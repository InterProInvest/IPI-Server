using HES.Core.Entities;
using HES.Core.Models;
using HES.Core.Interfaces;
using HES.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Core.Services
{
    public class SharedAccountService : ISharedAccountService
    {
        private readonly IAsyncRepository<SharedAccount> _sharedAccountRepository;
        private readonly IDeviceAccountService _deviceAccountService;
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly IDataProtectionService _dataProtectionService;

        public SharedAccountService(IAsyncRepository<SharedAccount> sharedAccountRepository,
                                    IDeviceAccountService deviceAccountService,
                                    IDeviceTaskService deviceTaskService,
                                    IDataProtectionService dataProtectionService)
        {
            _sharedAccountRepository = sharedAccountRepository;
            _deviceAccountService = deviceAccountService;
            _deviceTaskService = deviceTaskService;
            _dataProtectionService = dataProtectionService;
        }

        public IQueryable<SharedAccount> Query()
        {
            return _sharedAccountRepository.Query();
        }

        public async Task<SharedAccount> GetByIdAsync(dynamic id)
        {
            return await _sharedAccountRepository.GetByIdAsync(id);
        }

        public async Task<List<SharedAccount>> GetSharedAccountsAsync()
        {
            return await _sharedAccountRepository
                .Query()
                .Where(d => d.Deleted == false)
                .ToListAsync();
        }

        public async Task<SharedAccount> CreateSharedAccountAsync(SharedAccount sharedAccount)
        {
            _dataProtectionService.Validate();

            if (sharedAccount == null)
            {
                throw new ArgumentNullException(nameof(sharedAccount));
            }

            ValidationHepler.VerifyOtpSecret(sharedAccount.OtpSecret);

            var exist = await _sharedAccountRepository
                .Query()
                .Where(s => s.Name == sharedAccount.Name &&
                            s.Login == sharedAccount.Login &&
                            s.Deleted == false)
                .AsNoTracking()
                .AnyAsync();

            if (exist)
            {
                throw new Exception("An account with the same name and login exists.");
            }

            // Validate url
            sharedAccount.Urls = ValidationHepler.VerifyUrls(sharedAccount.Urls);

            // Set password
            sharedAccount.Password = _dataProtectionService.Encrypt(sharedAccount.Password);
            // Set password date change
            sharedAccount.PasswordChangedAt = DateTime.UtcNow;
            // Set otp date change
            if (!string.IsNullOrWhiteSpace(sharedAccount.OtpSecret))
            {
                sharedAccount.OtpSecret = _dataProtectionService.Encrypt(sharedAccount.OtpSecret);
                sharedAccount.OtpSecretChangedAt = DateTime.UtcNow;
            }

            return await _sharedAccountRepository.AddAsync(sharedAccount);
        }

        public async Task<SharedAccount> CreateWorkstationSharedAccountAsync(WorkstationAccount workstationAccount)
        {
            if (workstationAccount == null)
            {
                throw new ArgumentNullException(nameof(workstationAccount));
            }

            var sharedAccount = new SharedAccount()
            {
                Name = workstationAccount.Name,
                Kind = AccountKind.Workstation,
                Password = workstationAccount.Password
            };

            switch (workstationAccount.AccountType)
            {
                case WorkstationAccountType.Local:
                    sharedAccount.Login = $".\\{workstationAccount.Login}";
                    break;
                case WorkstationAccountType.Domain:
                    sharedAccount.Login = $"{workstationAccount.Domain}\\{workstationAccount.Login}";
                    break;
                case WorkstationAccountType.Microsoft:
                    sharedAccount.Login = $"@\\{workstationAccount.Login}";
                    break;
                case WorkstationAccountType.AzureAD:
                    sharedAccount.Login = $"AzureAD\\{workstationAccount.Login}";
                    break;
            }

            return await CreateSharedAccountAsync(sharedAccount);
        }

        public async Task<List<string>> EditSharedAccountAsync(SharedAccount sharedAccount)
        {
            _dataProtectionService.Validate();

            if (sharedAccount == null)
            {
                throw new Exception("The parameter must not be null.");
            }

            var exist = await _sharedAccountRepository
                .Query()
                .Where(s => s.Name == sharedAccount.Name)
                .Where(s => s.Login == sharedAccount.Login)
                .Where(s => s.Deleted == false)
                .Where(s => s.Id != sharedAccount.Id)
                .AnyAsync();

            if (exist)
            {
                throw new Exception("An account with the same name and login exists.");
            }

            // Validate url
            if (sharedAccount.Urls != null)
            {
                sharedAccount.Urls = ValidationHepler.VerifyUrls(sharedAccount.Urls);
            }

            // Get all device accounts where equals this shared account
            var deviceAccounts = await _deviceAccountService
                .Query()
                .Where(d => d.Deleted == false)
                .Where(d => d.SharedAccountId == sharedAccount.Id)
                .ToListAsync();

            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var deviceAccount in deviceAccounts)
            {
                deviceAccount.Name = sharedAccount.Name;
                deviceAccount.Urls = sharedAccount.Urls;
                deviceAccount.Apps = sharedAccount.Apps;
                deviceAccount.Login = sharedAccount.Login;
                deviceAccount.Status = AccountStatus.Updating;
                deviceAccount.UpdatedAt = DateTime.UtcNow;

                // Add Device Task
                tasks.Add(new DeviceTask
                {
                    DeviceAccountId = deviceAccount.Id,
                    OldName = sharedAccount.Name,
                    OldUrls = sharedAccount.Urls ?? string.Empty,
                    OldApps = sharedAccount.Apps ?? string.Empty,
                    OldLogin = sharedAccount.Login,
                    Password = null,
                    OtpSecret = null,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Update,
                    DeviceId = deviceAccount.DeviceId
                });
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Update Shared Account        
                await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, new string[] { "Name", "Urls", "Apps", "Login" });

                // Update device accounts
                await _deviceAccountService.UpdateOnlyPropAsync(deviceAccounts, new string[] { "Name", "Login", "Urls", "Apps", "Status", "UpdatedAt" });
                // Create Tasks
                await _deviceTaskService.AddRangeTasksAsync(tasks);

                transactionScope.Complete();
            }

            return deviceAccounts.Select(s => s.DeviceId).ToList();
        }

        public async Task<List<string>> EditSharedAccountPwdAsync(SharedAccount sharedAccount)
        {
            if (sharedAccount == null)
            {
                throw new ArgumentNullException(nameof(sharedAccount));
            }

            _dataProtectionService.Validate();

            // Update Shared Account
            sharedAccount.Password = _dataProtectionService.Encrypt(sharedAccount.Password);
            sharedAccount.PasswordChangedAt = DateTime.UtcNow;

            // Get all device accounts where equals this shared account
            var deviceAccounts = await _deviceAccountService
                .Query()
                .Where(d => d.Deleted == false)
                .Where(d => d.SharedAccountId == sharedAccount.Id)
                .ToListAsync();

            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var deviceAccount in deviceAccounts)
            {
                deviceAccount.Status = AccountStatus.Updating;
                deviceAccount.UpdatedAt = DateTime.UtcNow;
                deviceAccount.PasswordUpdatedAt = DateTime.UtcNow;

                // Add Device Task
                tasks.Add(new DeviceTask
                {
                    DeviceAccountId = deviceAccount.Id,
                    Password = sharedAccount.Password,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Update,
                    DeviceId = deviceAccount.DeviceId
                });
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Update Shared Account
                await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, new string[] { "Password", "PasswordChangedAt" });
                // Update device accounts
                await _deviceAccountService.UpdateOnlyPropAsync(deviceAccounts, new string[] { "Status", "UpdatedAt", "PasswordUpdatedAt" });
                // Create Tasks
                await _deviceTaskService.AddRangeTasksAsync(tasks);

                transactionScope.Complete();
            }

            var devices = deviceAccounts.Select(s => s.DeviceId).ToList();
            return devices;
        }

        public async Task<List<string>> EditSharedAccountOtpAsync(SharedAccount sharedAccount)
        {
            if (sharedAccount == null)
            {
                throw new ArgumentNullException(nameof(sharedAccount));
            }

            _dataProtectionService.Validate();

            ValidationHepler.VerifyOtpSecret(sharedAccount.OtpSecret);

            // Update Shared Account
            sharedAccount.OtpSecret = !string.IsNullOrWhiteSpace(sharedAccount.OtpSecret) ? _dataProtectionService.Encrypt(sharedAccount.OtpSecret) : null;
            sharedAccount.OtpSecretChangedAt = !string.IsNullOrWhiteSpace(sharedAccount.OtpSecret) ? new DateTime?(DateTime.UtcNow) : null;

            // Get all device accounts where equals this shared account
            var deviceAccounts = await _deviceAccountService
                .Query()
                .Where(d => d.Deleted == false)
                .Where(d => d.SharedAccountId == sharedAccount.Id)
                .ToListAsync();

            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var deviceAccount in deviceAccounts)
            {
                deviceAccount.Status = AccountStatus.Updating;
                deviceAccount.UpdatedAt = DateTime.UtcNow;
                deviceAccount.OtpUpdatedAt = sharedAccount.OtpSecretChangedAt;

                // Add Device Task
                tasks.Add(new DeviceTask
                {
                    DeviceAccountId = deviceAccount.Id,
                    OtpSecret = sharedAccount.OtpSecret ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Update,
                    DeviceId = deviceAccount.DeviceId
                });
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Update Shared Account
                await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, new string[] { "OtpSecret", "OtpSecretChangedAt" });

                // Update device accounts
                await _deviceAccountService.UpdateOnlyPropAsync(deviceAccounts, new string[] { "Status", "UpdatedAt", "OtpUpdatedAt" });
                // Create Tasks
                await _deviceTaskService.AddRangeTasksAsync(tasks);

                transactionScope.Complete();
            }

            return deviceAccounts.Select(s => s.DeviceId).ToList();
        }

        public async Task<List<string>> DeleteSharedAccountAsync(string id)
        {
            _dataProtectionService.Validate();

            if (id == null)
            {
                throw new Exception("The parameter must not be null.");
            }

            var sharedAccount = await _sharedAccountRepository.GetByIdAsync(id);
            if (sharedAccount == null)
            {
                throw new Exception("Shared account does not exist.");
            }

            sharedAccount.Deleted = true;
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, new string[] { "Deleted" });

            // Get all device accounts where equals this shared account
            var deviceAccounts = await _deviceAccountService
                .Query()
                .Where(d => d.Deleted == false)
                .Where(d => d.SharedAccountId == sharedAccount.Id)
                .ToListAsync();

            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var deviceAccount in deviceAccounts)
            {
                deviceAccount.Status = AccountStatus.Removing;
                deviceAccount.UpdatedAt = DateTime.UtcNow;

                // Add Device Task
                tasks.Add(new DeviceTask
                {
                    DeviceAccountId = deviceAccount.Id,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Delete,
                    DeviceId = deviceAccount.DeviceId
                });
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Update device accounts
                await _deviceAccountService.UpdateOnlyPropAsync(deviceAccounts, new string[] { "Status", "UpdatedAt" });
                // Create Tasks
                await _deviceTaskService.AddRangeTasksAsync(tasks);

                transactionScope.Complete();
            }

            return deviceAccounts.Select(s => s.DeviceId).ToList();
        }

        public async Task<bool> ExistAync(Expression<Func<SharedAccount, bool>> predicate)
        {
            return await _sharedAccountRepository.ExistAsync(predicate);
        }
    }
}