using HES.Core.Crypto;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

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
        public IServiceProvider Services { get; }
        private readonly ILogger<DataProtectionService> _logger;

        private DataProtectionKey _key;
        private bool _protectionEnabled;
        private bool _protectionBusy;
        private bool _protectionActivated;
        private bool _notFinishedPasswordChange;

        public DataProtectionService(IServiceProvider services, ILogger<DataProtectionService> logger)
        {
            Services = services;
            _logger = logger;
        }

        public async Task InitializeAsync()
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
                {
                    using var scope = Services.CreateScope();
                    var scopedEmailSenderService = scope.ServiceProvider.GetRequiredService<IEmailSenderService>();
                    await scopedEmailSenderService.SendActivateDataProtectionAsync();
                }
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

                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await DeleteDataProtectionEntity(key.KeyId);
                    await DecryptDatabase(key);
                    transactionScope.Complete();
                }

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

                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
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
                    transactionScope.Complete();
                }

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
            using var scope = Services.CreateScope();
            var scopedDeviceRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<HardwareVault>>();
            var scopedDeviceTaskRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<HardwareVaultTask>>();
            var scopedSharedAccountRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<SharedAccount>>();

            var devices = await scopedDeviceRepository.Query().ToListAsync();
            var deviceTasks = await scopedDeviceTaskRepository.Query().ToListAsync();
            var sharedAccounts = await scopedSharedAccountRepository.Query().ToListAsync();

            foreach (var device in devices)
            {
                if (device.MasterPassword != null)
                {
                    var plainText = key.Decrypt(device.MasterPassword);
                    device.MasterPassword = newKey.Encrypt(plainText);
                }
            }

            foreach (var task in deviceTasks)
            {
                if (task.Password != null)
                {
                    var plainText = key.Decrypt(task.Password);
                    task.Password = newKey.Encrypt(plainText);
                }
                if (task.OtpSecret != null)
                {
                    var plainText = key.Decrypt(task.OtpSecret);
                    task.OtpSecret = newKey.Encrypt(plainText);
                }
            }

            foreach (var account in sharedAccounts)
            {
                if (account.Password != null)
                {
                    var plainText = key.Decrypt(account.Password);
                    account.Password = newKey.Encrypt(plainText);
                }
                if (account.OtpSecret != null)
                {
                    var plainText = key.Decrypt(account.OtpSecret);
                    account.OtpSecret = newKey.Encrypt(plainText);
                }
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await scopedDeviceRepository.UpdateOnlyPropAsync(devices, new string[] { "MasterPassword" });
                await scopedDeviceTaskRepository.UpdateOnlyPropAsync(deviceTasks, new string[] { "Password", "OtpSecret" });
                await scopedSharedAccountRepository.UpdateOnlyPropAsync(sharedAccounts, new string[] { "Password", "OtpSecret" });
                transactionScope.Complete();
            }
        }

        private async Task EncryptDatabase(DataProtectionKey key)
        {
            using var scope = Services.CreateScope();
            var scopedDeviceRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<HardwareVault>>();
            var scopedDeviceTaskRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<HardwareVaultTask>>();
            var scopedSharedAccountRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<SharedAccount>>();

            var devices = await scopedDeviceRepository.Query().ToListAsync();
            var deviceTasks = await scopedDeviceTaskRepository.Query().ToListAsync();
            var sharedAccounts = await scopedSharedAccountRepository.Query().ToListAsync();

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

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await scopedDeviceRepository.UpdateOnlyPropAsync(devices, new string[] { "MasterPassword" });
                await scopedDeviceTaskRepository.UpdateOnlyPropAsync(deviceTasks, new string[] { "Password", "OtpSecret" });
                await scopedSharedAccountRepository.UpdateOnlyPropAsync(sharedAccounts, new string[] { "Password", "OtpSecret" });
                transactionScope.Complete();
            }
        }

        private async Task DecryptDatabase(DataProtectionKey key)
        {
            using var scope = Services.CreateScope();
            var scopedDeviceRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<HardwareVault>>();
            var scopedDeviceTaskRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<HardwareVaultTask>>();
            var scopedSharedAccountRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<SharedAccount>>();

            var devices = await scopedDeviceRepository.Query().ToListAsync();
            var deviceTasks = await scopedDeviceTaskRepository.Query().ToListAsync();
            var sharedAccounts = await scopedSharedAccountRepository.Query().ToListAsync();

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

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await scopedDeviceRepository.UpdateOnlyPropAsync(devices, new string[] { "MasterPassword" });
                await scopedDeviceTaskRepository.UpdateOnlyPropAsync(deviceTasks, new string[] { "Password", "OtpSecret" });
                await scopedSharedAccountRepository.UpdateOnlyPropAsync(sharedAccounts, new string[] { "Password", "OtpSecret" });
                transactionScope.Complete();
            }
        }

        private string TryGetStoredPassword()
        {
            using var scope = Services.CreateScope();
            var scopedConfiguration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            return scopedConfiguration.GetValue<string>("DataProtection:Password");
        }

        #region Data Protection Params

        private async Task<DataProtection> ReadDataProtectionEntity()
        {
            using var scope = Services.CreateScope();
            var scopedDataProtectionRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<DataProtection>>();

            var list = await scopedDataProtectionRepository
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
            using var scope = Services.CreateScope();
            var scopedDataProtectionRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<DataProtection>>();

            var entity = await scopedDataProtectionRepository.AddAsync(new DataProtection()
            {
                Value = JsonConvert.SerializeObject(prms),
                Params = prms
            });

            return entity;
        }

        private async Task DeleteDataProtectionEntity(int id)
        {
            using var scope = Services.CreateScope();
            var scopedDataProtectionRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<DataProtection>>();

            var entity = await scopedDataProtectionRepository
                .Query()
                .Where(v => v.Id == id)
                .FirstOrDefaultAsync();

            if (entity != null)
                await scopedDataProtectionRepository.DeleteAsync(entity);
        }

        #endregion
    }
}