using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public enum ProtectionStatus
    {
        None,
        On,
        Off,
        Busy,
        RequiresActivation
    }

    public class DataProtectionService : IDataProtectionService
    {
        private readonly IAsyncRepository<DataProtection> _dataProtectionRepository;
        private readonly IAsyncRepository<Device> _deviceRepository;
        private readonly IAsyncRepository<DeviceTask> _deviceTaskRepository;
        private readonly IAsyncRepository<SharedAccount> _sharedAccountRepository;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<DataProtectionService> _logger;
        private IDataProtector _dataProtector;
        private bool _enabledProtection;
        private bool _activatedProtection;
        private bool _isBusy;

        public DataProtectionService(IAsyncRepository<DataProtection> dataProtectionRepository,
                                     IAsyncRepository<Device> deviceRepository,
                                     IAsyncRepository<DeviceTask> deviceTaskRepository,
                                     IAsyncRepository<SharedAccount> sharedAccountRepository,
                                     IDataProtectionProvider dataProtectionProvider,
                                     ILogger<DataProtectionService> logger)
        {
            _dataProtectionRepository = dataProtectionRepository;
            _deviceRepository = deviceRepository;
            _deviceTaskRepository = deviceTaskRepository;
            _sharedAccountRepository = sharedAccountRepository;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public async Task<ProtectionStatus> Status()
        {
            var dataProtection = await _dataProtectionRepository.GetByIdAsync(DataProtection.PrimaryKey);

            if (dataProtection != null)
            {
                _enabledProtection = true;

                if (!_activatedProtection)
                {
                    return ProtectionStatus.RequiresActivation;
                }

                if (_isBusy)
                {
                    return ProtectionStatus.Busy;
                }

                return ProtectionStatus.On;
            }
            else
            {
                _enabledProtection = false;
                return ProtectionStatus.Off;
            }
        }

        public bool CanUse()
        {
            if (_enabledProtection)
            {
                if (_activatedProtection && !_isBusy)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        public void Validate()
        {
            if (!CanUse())
                throw new Exception("Data protection is not activated or busy.");
        }

        public async Task ActivateDataProtectionAsync(string password, string user)
        {
            if (_isBusy)
            {
                throw new Exception("Data protection is busy.");
            }

            if (_activatedProtection)
            {
                throw new Exception("Data protection is already activated.");
            }

            try
            {
                // Temp protector
                var tempProtector = _dataProtectionProvider.CreateProtector(password);
                // Get value
                var dataProtection = await _dataProtectionRepository.GetByIdAsync(DataProtection.PrimaryKey);
                // If no error occurred during the decrypt, then the password is correct
                var unprotectedValue = tempProtector.Unprotect(dataProtection.Value);
                // Create protector
                _dataProtector = _dataProtectionProvider.CreateProtector(password);
                _activatedProtection = true;
                _logger.LogInformation($"Protection was activated by {user}.");
            }
            catch (CryptographicException)
            {
                throw new Exception($"Invalid password, was entered by {user}.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task EnableDataProtectionAsync(string password, string user)
        {
            if (password == null)
            {
                throw new Exception("The password must not be null.");
            }

            if (_isBusy)
            {
                throw new Exception("Data protection is busy.");
            }

            if (_enabledProtection || _activatedProtection)
            {
                throw new Exception("Data protection is already enabled.");
            }

            var dataProtection = await _dataProtectionRepository.GetByIdAsync(DataProtection.PrimaryKey);

            if (dataProtection != null)
            {
                throw new Exception("The password already added.");
            }

            try
            {
                _logger.LogInformation($"Data protection started by {user}");
                _isBusy = true;
                // Create protector
                _dataProtector = _dataProtectionProvider.CreateProtector(password);
                // Protect value
                var protectedValue = _dataProtector.Protect(Guid.NewGuid().ToString());
                await _dataProtectionRepository.AddAsync(new DataProtection() { Id = DataProtection.PrimaryKey, Value = protectedValue });

                await ProtectAllDataAsync();

                _enabledProtection = true;
                _activatedProtection = true;
                _isBusy = false;
                _logger.LogInformation($"Protection was enabled by {user}.");
            }
            catch (Exception ex)
            {
                _isBusy = false;
                _logger.LogError(ex.Message);
            }
        }

        public async Task DisableDataProtectionAsync(string password, string user)
        {
            if (_isBusy)
            {
                throw new Exception("Data protection is busy.");
            }

            if (!_enabledProtection || !_activatedProtection)
            {
                throw new Exception("Data protection is already disabled.");
            }

            try
            {
                _logger.LogInformation($"Data unprotection started by {user}");
                _isBusy = true;
                // Temp protector
                var tempProtector = _dataProtectionProvider.CreateProtector(password);
                // Get value
                var dataProtection = await _dataProtectionRepository.GetByIdAsync(DataProtection.PrimaryKey);
                // If no error occurred during the decrypt, then the password is correct
                var unprotectedValue = tempProtector.Unprotect(dataProtection.Value);
                // Unprotect
                await UnprotectAllDataAsync();
                // Disable protector
                _dataProtector = null;
                await _dataProtectionRepository.DeleteAsync(dataProtection);

                _enabledProtection = false;
                _activatedProtection = false;
                _isBusy = false;
                _logger.LogInformation($"Protection was disabled by {user}.");
            }
            catch (CryptographicException)
            {
                _isBusy = false;
                throw new Exception($"Invalid password, was entered by {user}.");
            }
            catch (Exception ex)
            {
                _isBusy = false;
                throw new Exception(ex.Message);
            }
        }

        public async Task ChangeDataProtectionPasswordAsync(string oldPassword, string newPassword, string user)
        {
            if (oldPassword == null || newPassword == null)
            {
                throw new Exception("Password must not be null.");
            }

            if (_isBusy)
            {
                throw new Exception("Data protection is busy.");
            }

            if (!_enabledProtection || !_activatedProtection)
            {
                throw new Exception("Data protection is disabled.");
            }

            try
            {
                _logger.LogInformation($"Protection password start change by {user}.");
                _isBusy = true;
                // Temp protector
                var tempProtector = _dataProtectionProvider.CreateProtector(oldPassword);
                // Get value
                var dataProtection = await _dataProtectionRepository.GetByIdAsync(DataProtection.PrimaryKey);
                // If no error occurred during the decrypt, then the password is correct
                var unprotectedValue = tempProtector.Unprotect(dataProtection.Value);
                // Unprotect all
                await UnprotectAllDataAsync();
                // Create protector
                _dataProtector = _dataProtectionProvider.CreateProtector(newPassword);
                // Protect value
                var protectedValue = _dataProtector.Protect(Guid.NewGuid().ToString());
                dataProtection.Value = protectedValue;
                await _dataProtectionRepository.UpdateOnlyPropAsync(dataProtection, new string[] { "Value" });
                // Protect all
                await ProtectAllDataAsync();
                _isBusy = false;
                _logger.LogInformation($"Protection password was changed by {user}.");
            }
            catch (CryptographicException)
            {
                _isBusy = false;
                throw new Exception($"Invalid password, was entered by {user}.");
            }
            catch (Exception ex)
            {
                _isBusy = false;
                throw new Exception(ex.Message);
            }
        }

        public string Protect(string text)
        {
            if (text == null)
                return null;

            if (_enabledProtection)
            {
                if (!_activatedProtection)
                {
                    throw new Exception("Data protection not activated");
                }
                if (_isBusy)
                {
                    throw new Exception("Data protection is busy");
                }
                return _dataProtector.Protect(text);
            }
            return text;
        }

        public string Unprotect(string text)
        {
            if (text == null)
                return null;

            if (_enabledProtection)
            {
                if (!_activatedProtection)
                {
                    throw new Exception("Data protection not activated");
                }
                if (_isBusy)
                {
                    throw new Exception("Data protection is busy");
                }
                return _dataProtector.Unprotect(text);
            }
            return text;
        }

        private async Task ProtectAllDataAsync()
        {
            _logger.LogDebug($"Devices stage");
            var devices = await _deviceRepository.Query().ToListAsync();
            foreach (var device in devices)
            {
                if (device.MasterPassword != null)
                {
                    device.MasterPassword = _dataProtector.Protect(device.MasterPassword);
                    await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "MasterPassword" });
                }
            }

            _logger.LogDebug($"DeviceTasks stage");
            var deviceTasks = await _deviceTaskRepository.Query().ToListAsync();
            foreach (var task in deviceTasks)
            {
                var taskProperties = new List<string>();
                if (task.Password != null)
                {
                    task.Password = _dataProtector.Protect(task.Password);
                    taskProperties.Add("Password");
                }
                if (task.OtpSecret != null)
                {
                    task.OtpSecret = _dataProtector.Protect(task.OtpSecret);
                    taskProperties.Add("OtpSecret");
                }
                await _deviceTaskRepository.UpdateOnlyPropAsync(task, taskProperties.ToArray());
            }

            _logger.LogDebug($"SharedAccounts stage");
            var sharedAccounts = await _sharedAccountRepository.Query().ToListAsync();
            foreach (var account in sharedAccounts)
            {
                var accountProperties = new List<string>();
                if (account.Password != null)
                {
                    account.Password = _dataProtector.Protect(account.Password);
                    accountProperties.Add("Password");
                }
                if (account.OtpSecret != null)
                {
                    account.OtpSecret = _dataProtector.Protect(account.OtpSecret);
                    accountProperties.Add("OtpSecret");
                }
                await _sharedAccountRepository.UpdateOnlyPropAsync(account, accountProperties.ToArray());
            }
        }

        private async Task UnprotectAllDataAsync()
        {
            try
            {
                _logger.LogDebug($"Devices stage");
                var devices = await _deviceRepository.Query().ToListAsync();
                foreach (var device in devices)
                {
                    if (device.MasterPassword != null)
                    {
                        device.MasterPassword = _dataProtector.Unprotect(device.MasterPassword);
                        await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "MasterPassword" });
                    }
                }

                _logger.LogDebug($"DeviceTasks stage");
                var deviceTasks = await _deviceTaskRepository.Query().ToListAsync();
                foreach (var task in deviceTasks)
                {
                    var taskProperties = new List<string>();
                    if (task.Password != null)
                    {
                        task.Password = _dataProtector.Unprotect(task.Password);
                        taskProperties.Add("Password");
                    }
                    if (task.OtpSecret != null)
                    {
                        task.OtpSecret = _dataProtector.Unprotect(task.OtpSecret);
                        taskProperties.Add("OtpSecret");
                    }
                    await _deviceTaskRepository.UpdateOnlyPropAsync(task, taskProperties.ToArray());
                }

                _logger.LogDebug($"SharedAccounts stage");
                var sharedAccounts = await _sharedAccountRepository.Query().ToListAsync();
                foreach (var account in sharedAccounts)
                {
                    var accountProperties = new List<string>();
                    if (account.Password != null)
                    {
                        account.Password = _dataProtector.Unprotect(account.Password);
                        accountProperties.Add("Password");
                    }
                    if (account.OtpSecret != null)
                    {
                        account.OtpSecret = _dataProtector.Unprotect(account.OtpSecret);
                        accountProperties.Add("OtpSecret");
                    }
                    await _sharedAccountRepository.UpdateOnlyPropAsync(account, accountProperties.ToArray());
                }
            }
            catch (CryptographicException)
            {
                throw new Exception("Unprotection error, data was protected with another key.");
            }
        }
    }
}