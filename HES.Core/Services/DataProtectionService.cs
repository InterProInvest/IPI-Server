using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Crypto;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

//todo - use transactions when enabling/disabling the protection

namespace HES.Core.Services
{
    public enum ProtectionStatus
    {
        Off,
        On,
        Busy,
        Activate,
        NotFinishedPasswordChange
    }

    public class DataProtectionService : IDataProtectionService
    {
        private readonly IConfiguration _config;
        private readonly IAsyncRepository<DataProtection> _dataProtectionRepository;
        private readonly IAsyncRepository<Device> _deviceRepository;
        private readonly IAsyncRepository<DeviceTask> _deviceTaskRepository;
        private readonly IAsyncRepository<SharedAccount> _sharedAccountRepository;
        private readonly IEmailSenderService _emailSenderService;
        private readonly ILogger<DataProtectionService> _logger;

        private DataProtectionKey _key;
        private bool _protectionEnabled;
        private bool _protectionBusy;
        private bool _protectionActivated;
        private bool _notFinishedPasswordChange;

        public DataProtectionService(IConfiguration config,
                                     IAsyncRepository<DataProtection> dataProtectionRepository,
                                     IAsyncRepository<Device> deviceRepository,
                                     IAsyncRepository<DeviceTask> deviceTaskRepository,
                                     IAsyncRepository<SharedAccount> sharedAccountRepository,
                                     IEmailSenderService emailSenderService,
                                     ILogger<DataProtectionService> logger)
        {
            _config = config;
            _dataProtectionRepository = dataProtectionRepository;
            _deviceRepository = deviceRepository;
            _deviceTaskRepository = deviceTaskRepository;
            _sharedAccountRepository = sharedAccountRepository;
            _emailSenderService = emailSenderService;
            _logger = logger;
        }

        public async Task Initialize()
        {
            _protectionEnabled = false;
            _protectionActivated = false;
            _notFinishedPasswordChange = false;

            try
            {
                var prms = await ReadDataProtectionEntity();

                if (prms != null)
                {
                    _protectionEnabled = true;

                    var password = TryGetStoredPassword();

                    if (password != null)
                    {
                        await ActivateProtectionAsync(password);
                    }
                }
            }
            catch (NotFinishedPasswordChangeException)
            {
                _protectionEnabled = true;
                _notFinishedPasswordChange = true;
            }
            finally
            {
                if (_protectionEnabled && !_protectionActivated)
                    await _emailSenderService.SendActivateDataProtectionAsync();
            }
        }

        public ProtectionStatus Status()
        {
            if (!_protectionEnabled)
                return ProtectionStatus.Off;

            if (_notFinishedPasswordChange)
                return ProtectionStatus.NotFinishedPasswordChange;

            if (!_protectionActivated)
                return ProtectionStatus.Activate;

            return ProtectionStatus.On;
        }

        public void Validate()
        {
            if (_notFinishedPasswordChange)
                throw new Exception("Data protection not finished password change.");

            if (_protectionEnabled && !_protectionActivated)
                throw new Exception("Data protection not activated.");

            if (_protectionBusy)
                throw new Exception("Data protection is busy.");
        }

        public string Encrypt(string plainText)
        {
            if (plainText == null)
                return null;

            if (!_protectionEnabled)
                return plainText;

            Validate();

            return _key.Encrypt(plainText);
        }

        public string Decrypt(string cipherText)
        {
            if (cipherText == null)
                return null;

            if (!_protectionEnabled)
                return cipherText;

            Validate();

            return _key.Decrypt(cipherText);
        }

        public async Task<bool> ActivateProtectionAsync(string password)
        {
            try
            {
                if (_protectionBusy)
                    throw new Exception("Data protection is busy.");
                _protectionBusy = true;

                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentNullException(nameof(password));

                if (_protectionActivated)
                    throw new Exception("Data protection is already activated.");

                var dataProtectionEntity = await ReadDataProtectionEntity();
                if (dataProtectionEntity == null)
                    throw new Exception("Data protection parameters not found.");

                _key = new DataProtectionKey(dataProtectionEntity.Id, dataProtectionEntity.Params);

                _protectionActivated = _key.ValidatePassword(password);

                return _protectionActivated;
            }
            finally
            {
                _protectionBusy = false;
            }
        }

        public async Task EnableProtectionAsync(string password)
        {
            try
            {
                if (_protectionBusy)
                    throw new Exception("Data protection is busy.");
                _protectionBusy = true;

                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentNullException(nameof(password));

                if (_protectionEnabled)
                    throw new Exception("Data protection is already enabled.");

                var prms = DataProtectionKey.CreateParams(password);

                var dataProtectionEntity = await SaveDataProtectionEntity(prms);

                _key = new DataProtectionKey(dataProtectionEntity.Id, dataProtectionEntity.Params);
                _key.ValidatePassword(password);

                await EncryptDatabase(_key);

                _protectionEnabled = true;
                _protectionActivated = true;
            }
            finally
            {
                _protectionBusy = false;
            }
        }

        public async Task DisableProtectionAsync(string password)
        {
            try
            {
                if (_protectionBusy)
                    throw new Exception("Data protection is busy.");
                _protectionBusy = true;

                if (!_protectionEnabled)
                    throw new Exception("Data protection is not enabled.");

                if (!_protectionActivated)
                    throw new Exception("Data protection is not activated.");


                var dataProtectionEntity = await ReadDataProtectionEntity();
                if (dataProtectionEntity == null)
                    throw new Exception("Data protection parameters not found.");

                var key = new DataProtectionKey(dataProtectionEntity.Id, dataProtectionEntity.Params);

                if (!key.ValidatePassword(password))
                    throw new Exception("Incorrect password");

                await DecryptDatabase(key);

                await DeleteDataProtectionEntity(key.KeyId);

                _key = null;
                _protectionActivated = false;
                _protectionEnabled = false;
            }
            finally
            {
                _protectionBusy = false;
            }
        }

        public async Task ChangeProtectionPasswordAsync(string oldPassword, string newPassword)
        {
            try
            {
                if (_protectionBusy)
                    throw new Exception("Data protection is busy.");
                _protectionBusy = true;

                if (!_protectionActivated)
                    throw new Exception("Data protection is not activated");

                var oldDataProtectionEntity = await ReadDataProtectionEntity();
                if (oldDataProtectionEntity == null)
                    throw new Exception("Data protection parameters not found.");

                var oldKey = new DataProtectionKey(oldDataProtectionEntity.Id, oldDataProtectionEntity.Params);

                if (!oldKey.ValidatePassword(oldPassword))
                    throw new Exception("Incorrect old password");

                // Creating the key for the new password
                var prms = DataProtectionKey.CreateParams(newPassword);
                var newDataProtectionEntity = await SaveDataProtectionEntity(prms);
                var newKey = new DataProtectionKey(newDataProtectionEntity.Id, newDataProtectionEntity.Params);
                newKey.ValidatePassword(newPassword);

                await ReencryptDatabase(oldKey, newKey);

                // Delete old key
                await DeleteDataProtectionEntity(oldKey.KeyId);

                // Set new key as a current key
                _key = newKey;

                // Set activated if detected the not finished password change operation.
                _protectionActivated = true;
            }
            finally
            {
                _protectionBusy = false;
            }
        }

        private async Task ReencryptDatabase(DataProtectionKey key, DataProtectionKey newKey)
        {
            var devices = await _deviceRepository.Query().ToListAsync();
            var deviceTasks = await _deviceTaskRepository.Query().ToListAsync();
            var sharedAccounts = await _sharedAccountRepository.Query().ToListAsync();

            foreach (var device in devices)
            {
                if (device.MasterPassword != null &&
                    key.TryDecrypt(device.MasterPassword, out string plainText))
                {
                    device.MasterPassword = newKey.Encrypt(plainText);
                }
            }

            foreach (var task in deviceTasks)
            {
                if (task.Password != null &&
                    key.TryDecrypt(task.Password, out string plainText))
                {
                    task.Password = newKey.Encrypt(plainText);
                }
                if (task.OtpSecret != null &&
                    key.TryDecrypt(task.OtpSecret, out plainText))
                {
                    task.OtpSecret = newKey.Encrypt(plainText);
                }
            }

            foreach (var account in sharedAccounts)
            {
                if (account.Password != null &&
                    key.TryDecrypt(account.Password, out string plainText))
                {
                    account.Password = newKey.Encrypt(plainText);
                }
                if (account.OtpSecret != null &&
                    key.TryDecrypt(account.OtpSecret, out plainText))
                {
                    account.OtpSecret = newKey.Encrypt(plainText);
                }
            }

            // devices validation - go through all records and check if each has a new key-id
            int correctRecords = 0;
            int wrongRecords = 0;
            foreach (var device in devices)
            {
                if (device.MasterPassword != null)
                {
                    if (newKey.TryDecrypt(device.MasterPassword, out string plainText))
                        correctRecords++;
                    else
                        wrongRecords++;
                }
            }

            if (wrongRecords > 0)
                _logger.LogInformation($"Password change for the devices: fail - {wrongRecords}");

            // deviceTasks validation
            correctRecords = 0;
            wrongRecords = 0;
            foreach (var task in deviceTasks)
            {
                if (task.Password != null)
                {
                    if (newKey.TryDecrypt(task.Password, out string plainText))
                        correctRecords++;
                    else
                        wrongRecords++;
                }
                if (task.OtpSecret != null)
                {
                    if (newKey.TryDecrypt(task.OtpSecret, out string plainText))
                        correctRecords++;
                    else
                        wrongRecords++;
                }
            }

            if (wrongRecords > 0)
                _logger.LogInformation($"Password change for the deviceTasks: fail - {wrongRecords}");

            // sharedAccounts validation
            correctRecords = 0;
            wrongRecords = 0;
            foreach (var account in sharedAccounts)
            {
                if (account.Password != null)
                {
                    if (newKey.TryDecrypt(account.Password, out string plainText))
                        correctRecords++;
                    else
                        wrongRecords++;
                }
                if (account.OtpSecret != null)
                {
                    if (newKey.TryDecrypt(account.OtpSecret, out string plainText))
                        correctRecords++;
                    else
                        wrongRecords++;
                }
            }

            if (wrongRecords > 0)
                _logger.LogError($"Password change for the sharedAccounts: fail - {wrongRecords}");

            await _deviceRepository.UpdateOnlyPropAsync(devices, new string[] { "MasterPassword" });
            await _deviceTaskRepository.UpdateOnlyPropAsync(deviceTasks, new string[] { "Password", "OtpSecret" });
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccounts, new string[] { "Password", "OtpSecret" });
        }

        private async Task EncryptDatabase(DataProtectionKey key)
        {
            var devices = await _deviceRepository.Query().ToListAsync();
            var deviceTasks = await _deviceTaskRepository.Query().ToListAsync();
            var sharedAccounts = await _sharedAccountRepository.Query().ToListAsync();

            foreach (var device in devices)
            {
                if (device.MasterPassword != null)
                    device.MasterPassword = key.Encrypt(device.MasterPassword);
            }

            foreach (var task in deviceTasks)
            {
                if (task.Password != null)
                    task.Password = key.Encrypt(task.Password);
                if (task.OtpSecret != null)
                    task.OtpSecret = key.Encrypt(task.OtpSecret);
            }

            foreach (var account in sharedAccounts)
            {
                if (account.Password != null)
                    account.Password = key.Encrypt(account.Password);
                if (account.OtpSecret != null)
                    account.OtpSecret = key.Encrypt(account.OtpSecret);
            }

            await _deviceRepository.UpdateOnlyPropAsync(devices, new string[] { "MasterPassword" });
            await _deviceTaskRepository.UpdateOnlyPropAsync(deviceTasks, new string[] { "Password", "OtpSecret" });
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccounts, new string[] { "Password", "OtpSecret" });
        }

        private async Task DecryptDatabase(DataProtectionKey key)
        {
            var devices = await _deviceRepository.Query().ToListAsync();
            var deviceTasks = await _deviceTaskRepository.Query().ToListAsync();
            var sharedAccounts = await _sharedAccountRepository.Query().ToListAsync();

            foreach (var device in devices)
            {
                if (device.MasterPassword != null)
                    device.MasterPassword = key.Decrypt(device.MasterPassword);
            }

            foreach (var task in deviceTasks)
            {
                if (task.Password != null)
                    task.Password = key.Decrypt(task.Password);
                if (task.OtpSecret != null)
                    task.OtpSecret = key.Decrypt(task.OtpSecret);
            }

            foreach (var account in sharedAccounts)
            {
                if (account.Password != null)
                    account.Password = key.Decrypt(account.Password);
                if (account.OtpSecret != null)
                    account.OtpSecret = key.Decrypt(account.OtpSecret);
            }

            await _deviceRepository.UpdateOnlyPropAsync(devices, new string[] { "MasterPassword" });
            await _deviceTaskRepository.UpdateOnlyPropAsync(deviceTasks, new string[] { "Password", "OtpSecret" });
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccounts, new string[] { "Password", "OtpSecret" });
        }

        private string TryGetStoredPassword()
        {
            return _config.GetValue<string>("DataProtection:Password");
        }

        #region Data Protection Params

        private async Task<DataProtection> ReadDataProtectionEntity()
        {
            var list = await _dataProtectionRepository
                .Query()
                .ToListAsync();

            if (list.Count == 0)
                return null;

            if (list.Count > 1)
                throw new NotFinishedPasswordChangeException("Detected not finished password change operation");

            var entity = list[0];
            if (string.IsNullOrEmpty(entity.Value))
                throw new Exception("Data Protection parameters is empty");

            entity.Params = JsonConvert.DeserializeObject<DataProtectionParams>(entity.Value);

            return entity;
        }

        private async Task<DataProtection> SaveDataProtectionEntity(DataProtectionParams prms)
        {
            var entity = await _dataProtectionRepository.AddAsync(new DataProtection()
            {
                Value = JsonConvert.SerializeObject(prms),
                Params = prms
            });

            return entity;
        }

        private async Task DeleteDataProtectionEntity(int id)
        {
            var entity = await _dataProtectionRepository
                .Query()
                .Where(v => v.Id == id)
                .FirstOrDefaultAsync();

            if (entity != null)
                await _dataProtectionRepository.DeleteAsync(entity);
        }

        #endregion
    }
}