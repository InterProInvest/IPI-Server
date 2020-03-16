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
        private readonly IAccountService _accountService;
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly IDataProtectionService _dataProtectionService;

        public SharedAccountService(IAsyncRepository<SharedAccount> sharedAccountRepository,
                                    IAccountService deviceAccountService,
                                    IDeviceTaskService deviceTaskService,
                                    IDataProtectionService dataProtectionService)
        {
            _sharedAccountRepository = sharedAccountRepository;
            _accountService = deviceAccountService;
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
            if (sharedAccount == null)
                throw new Exception("The parameter must not be null.");

            _dataProtectionService.Validate();

            var exist = await _sharedAccountRepository
                .Query()
                .Where(s => s.Name == sharedAccount.Name)
                .Where(s => s.Login == sharedAccount.Login)
                .Where(s => s.Deleted == false)
                .Where(s => s.Id != sharedAccount.Id)
                .AnyAsync();

            if (exist)
                throw new Exception("An account with the same name and login exists.");

            sharedAccount.Urls = ValidationHepler.VerifyUrls(sharedAccount.Urls);

            // Get all device accounts where equals this shared account
            var accounts = await _accountService
                .Query()
                .Include(d => d.Employee.Devices)
                .Where(d => d.Deleted == false && d.SharedAccountId == sharedAccount.Id)
                .ToListAsync();

            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var account in accounts)
            {
                account.Name = sharedAccount.Name;
                account.Urls = sharedAccount.Urls;
                account.Apps = sharedAccount.Apps;
                account.Login = sharedAccount.Login;
                account.UpdatedAt = DateTime.UtcNow;

                foreach (var device in account.Employee.Devices)
                {
                    tasks.Add(new DeviceTask
                    {
                        AccountId = account.Id,
                        CreatedAt = DateTime.UtcNow,
                        Operation = TaskOperation.Update,
                        DeviceId = device.Id
                    });
                }
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Update Shared Account        
                await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, new string[] { nameof(SharedAccount.Name), nameof(SharedAccount.Urls), nameof(SharedAccount.Apps), nameof(SharedAccount.Login) });
                // Update accounts
                await _accountService.UpdateOnlyPropAsync(accounts, new string[] { nameof(Account.Name), nameof(Account.Urls), nameof(Account.Apps), nameof(Account.Login), nameof(Account.UpdatedAt) });
                // Create Tasks
                await _deviceTaskService.AddRangeTasksAsync(tasks);

                transactionScope.Complete();
            }

            List<string> deviceIds = new List<string>();
            accounts.ForEach(x => deviceIds.AddRange(x.Employee.Devices.Select(s => s.Id)));
            return deviceIds;
        }

        public async Task<List<string>> EditSharedAccountPwdAsync(SharedAccount sharedAccount)
        {
            if (sharedAccount == null)
                throw new ArgumentNullException(nameof(sharedAccount));

            _dataProtectionService.Validate();

            // Update Shared Account
            sharedAccount.Password = _dataProtectionService.Encrypt(sharedAccount.Password);
            sharedAccount.PasswordChangedAt = DateTime.UtcNow;

            // Get all device accounts where equals this shared account
            var accounts = await _accountService
                .Query()
                .Where(d => d.Deleted == false)
                .Where(d => d.SharedAccountId == sharedAccount.Id)
                .ToListAsync();

            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var account in accounts)
            {
                account.UpdatedAt = DateTime.UtcNow;
                account.PasswordUpdatedAt = DateTime.UtcNow;

                foreach (var device in account.Employee.Devices)
                {
                    tasks.Add(new DeviceTask
                    {
                        Password = sharedAccount.Password,
                        AccountId = account.Id,
                        CreatedAt = DateTime.UtcNow,
                        Operation = TaskOperation.Update,
                        DeviceId = device.Id
                    });
                }
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Update Shared Account
                await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, new string[] { nameof(SharedAccount.Password), nameof(SharedAccount.PasswordChangedAt) });
                // Update accounts
                await _accountService.UpdateOnlyPropAsync(accounts, new string[] { nameof(Account.UpdatedAt), nameof(Account.PasswordUpdatedAt) });
                // Create Tasks
                await _deviceTaskService.AddRangeTasksAsync(tasks);

                transactionScope.Complete();
            }

            List<string> deviceIds = new List<string>();
            accounts.ForEach(x => deviceIds.AddRange(x.Employee.Devices.Select(s => s.Id)));
            return deviceIds;
        }

        public async Task<List<string>> EditSharedAccountOtpAsync(SharedAccount sharedAccount)
        {
            if (sharedAccount == null)
                throw new ArgumentNullException(nameof(sharedAccount));

            _dataProtectionService.Validate();

            ValidationHepler.VerifyOtpSecret(sharedAccount.OtpSecret);

            // Update Shared Account
            sharedAccount.OtpSecret = !string.IsNullOrWhiteSpace(sharedAccount.OtpSecret) ? _dataProtectionService.Encrypt(sharedAccount.OtpSecret) : null;
            sharedAccount.OtpSecretChangedAt = !string.IsNullOrWhiteSpace(sharedAccount.OtpSecret) ? new DateTime?(DateTime.UtcNow) : null;

            // Get all device accounts where equals this shared account
            var accounts = await _accountService
                .Query()
                .Where(d => d.Deleted == false)
                .Where(d => d.SharedAccountId == sharedAccount.Id)
                .ToListAsync();

            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var account in accounts)
            {
                account.UpdatedAt = DateTime.UtcNow;
                account.OtpUpdatedAt = sharedAccount.OtpSecretChangedAt;

                foreach (var device in account.Employee.Devices)
                {
                    tasks.Add(new DeviceTask
                    {
                        OtpSecret = sharedAccount.OtpSecret ?? string.Empty,
                        AccountId = account.Id,
                        CreatedAt = DateTime.UtcNow,
                        Operation = TaskOperation.Update,
                        DeviceId = device.Id
                    });
                }
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Update Shared Account
                await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, new string[] { nameof(SharedAccount.OtpSecret), nameof(SharedAccount.OtpSecretChangedAt) });
                // Update accounts
                await _accountService.UpdateOnlyPropAsync(accounts, new string[] { nameof(Account.UpdatedAt), nameof(Account.OtpUpdatedAt) });
                // Create Tasks
                await _deviceTaskService.AddRangeTasksAsync(tasks);

                transactionScope.Complete();
            }

            List<string> deviceIds = new List<string>();
            accounts.ForEach(x => deviceIds.AddRange(x.Employee.Devices.Select(s => s.Id)));
            return deviceIds;
        }

        public async Task<List<string>> DeleteSharedAccountAsync(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            _dataProtectionService.Validate();

            var sharedAccount = await _sharedAccountRepository.GetByIdAsync(id);
            if (sharedAccount == null)
                throw new Exception("Shared Account not found");

            sharedAccount.Deleted = true;
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, new string[] { nameof(SharedAccount.Name) });

            // Get all device accounts where equals this shared account
            var accounts = await _accountService
                .Query()
                .Where(d => d.Deleted == false)
                .Where(d => d.SharedAccountId == sharedAccount.Id)
                .ToListAsync();

            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var account in accounts)
            {
                account.Deleted = true;
                account.UpdatedAt = DateTime.UtcNow;

                foreach (var device in account.Employee.Devices)
                {
                    tasks.Add(new DeviceTask
                    {
                        AccountId = account.Id,
                        CreatedAt = DateTime.UtcNow,
                        Operation = TaskOperation.Update,
                        DeviceId = device.Id
                    });
                }
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Update accounts
                await _accountService.UpdateOnlyPropAsync(accounts, new string[] { nameof(Account.Deleted), nameof(Account.UpdatedAt) });
                // Create Tasks
                await _deviceTaskService.AddRangeTasksAsync(tasks);

                transactionScope.Complete();
            }

            List<string> deviceIds = new List<string>();
            accounts.ForEach(x => deviceIds.AddRange(x.Employee.Devices.Select(s => s.Id)));
            return deviceIds;
        }

        public async Task<bool> ExistAync(Expression<Func<SharedAccount, bool>> predicate)
        {
            return await _sharedAccountRepository.ExistAsync(predicate);
        }
    }
}