using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Models;
using HES.Core.Interfaces;
using HES.Core.Utilities;
using Hideez.SDK.Communication.Security;
using Hideez.SDK.Communication.Utils;
using Microsoft.EntityFrameworkCore;
using HES.Core.Enums;
using System.Transactions;
using HES.Core.Exceptions;

namespace HES.Core.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IAsyncRepository<Employee> _employeeRepository;
        private readonly IHardwareVaultService _deviceService;
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly IAccountService _accountService;
        private readonly ISharedAccountService _sharedAccountService;
        private readonly IWorkstationService _workstationService;
        private readonly ILicenseService _licenseService;
        private readonly IAsyncRepository<WorkstationEvent> _workstationEventRepository;
        private readonly IAsyncRepository<WorkstationSession> _workstationSessionRepository;
        private readonly IDataProtectionService _dataProtectionService;

        public EmployeeService(IAsyncRepository<Employee> employeeRepository,
                               IHardwareVaultService deviceService,
                               IDeviceTaskService deviceTaskService,
                               IAccountService deviceAccountService,
                               ISharedAccountService sharedAccountService,
                               IWorkstationService workstationService,
                               ILicenseService licenseService,
                               IAsyncRepository<WorkstationEvent> workstationEventRepository,
                               IAsyncRepository<WorkstationSession> workstationSessionRepository,
                               IDataProtectionService dataProtectionService)
        {
            _employeeRepository = employeeRepository;
            _deviceService = deviceService;
            _deviceTaskService = deviceTaskService;
            _accountService = deviceAccountService;
            _sharedAccountService = sharedAccountService;
            _workstationService = workstationService;
            _licenseService = licenseService;
            _workstationEventRepository = workstationEventRepository;
            _workstationSessionRepository = workstationSessionRepository;
            _dataProtectionService = dataProtectionService;
        }

        #region Employee

        public IQueryable<Employee> EmployeeQuery()
        {
            return _employeeRepository.Query();
        }

        public async Task<Employee> GetEmployeeByIdAsync(string id)
        {
            return await _employeeRepository
                .Query()
                .Include(e => e.Department.Company)
                .Include(e => e.Position)
                .Include(e => e.SoftwareVaults)
                .Include(e => e.SoftwareVaultInvitations)
                .Include(e => e.HardwareVaults)
                .ThenInclude(e => e.HardwareVaultProfile)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Employee> GetEmployeeByFullNameAsync(Employee employee)
        {
            return await _employeeRepository
                .Query()
                .Include(e => e.Department.Company)
                .Include(e => e.Position)
                .Include(e => e.HardwareVaults)
                .ThenInclude(e => e.HardwareVaultProfile)
                .FirstOrDefaultAsync(x => x.FirstName == employee.FirstName &&
                                          x.LastName == employee.LastName);
        }

        public async Task<List<Employee>> GetEmployeesAsync()
        {
            return await _employeeRepository
                .Query()
                .Include(e => e.Department.Company)
                .Include(e => e.Position)
                .Include(e => e.HardwareVaults)
                .ToListAsync();
        }

        public async Task<IList<string>> GetEmployeeDevicesAsync(string employeeId)
        {
            var employee = await GetEmployeeByIdAsync(employeeId);
            return employee.HardwareVaults.Select(x => x.Id).ToList();
        }

        public async Task<List<Employee>> GetFilteredEmployeesAsync(EmployeeFilter employeeFilter)
        {
            var filter = _employeeRepository
                .Query()
                .Include(e => e.Department.Company)
                .Include(e => e.Position)
                .Include(e => e.HardwareVaults)
                .AsQueryable();

            if (employeeFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Department.Company.Id == employeeFilter.CompanyId);
            }
            if (employeeFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.DepartmentId == employeeFilter.DepartmentId);
            }
            if (employeeFilter.PositionId != null)
            {
                filter = filter.Where(w => w.PositionId == employeeFilter.PositionId);
            }
            if (employeeFilter.DevicesCount != null)
            {
                filter = filter.Where(w => w.HardwareVaults.Count() == employeeFilter.DevicesCount);
            }
            if (employeeFilter.StartDate != null && employeeFilter.EndDate != null)
            {
                filter = filter.Where(w => w.LastSeen.HasValue
                                        && w.LastSeen.Value >= employeeFilter.StartDate.Value.AddSeconds(0).AddMilliseconds(0).ToUniversalTime()
                                        && w.LastSeen.Value <= employeeFilter.EndDate.Value.AddSeconds(59).AddMilliseconds(999).ToUniversalTime());
            }

            return await filter
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Take(employeeFilter.Records)
                .ToListAsync();
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            // If the field is NULL then the unique check does not work; therefore, we write empty
            employee.LastName = employee.LastName ?? string.Empty;

            var exist = await _employeeRepository.ExistAsync(x => x.FirstName == employee.FirstName && x.LastName == employee.LastName);
            if (exist)
            {
                throw new AlreadyExistException($"{employee.FirstName} {employee.LastName} already exists.");
            }

            return await _employeeRepository.AddAsync(employee);
        }

        public async Task EditEmployeeAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            // If the field is NULL then the unique check does not work; therefore, we write empty
            employee.LastName = employee.LastName ?? string.Empty;

            var exist = await _employeeRepository.ExistAsync(x => x.FirstName == employee.FirstName && x.LastName == employee.LastName && x.Id != employee.Id);
            if (exist)
            {
                throw new AlreadyExistException($"{employee.FirstName} {employee.LastName} already exists.");
            }

            var properties = new string[] { "FirstName", "LastName", "Email", "PhoneNumber", "DepartmentId", "PositionId" };
            await _employeeRepository.UpdateOnlyPropAsync(employee, properties);
        }

        public async Task DeleteEmployeeAsync(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var employee = await _employeeRepository.GetByIdAsync(id);

            if (employee == null)
                throw new Exception("Employee not found");

            var devices = await _deviceService
                .VaultQuery()
                .Where(x => x.EmployeeId == id)
                .AnyAsync();

            if (devices)
            {
                throw new Exception("The employee has a device attached, first untie the device before removing.");
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Remove all events
                var allEvents = await _workstationEventRepository.Query().Where(e => e.EmployeeId == id).ToListAsync();
                await _workstationEventRepository.DeleteRangeAsync(allEvents);
                // Remove all sessions
                var allSessions = await _workstationSessionRepository.Query().Where(s => s.EmployeeId == id).ToListAsync();
                await _workstationSessionRepository.DeleteRangeAsync(allSessions);
                // Remove all accounts
                await _accountService.RemoveAllAccountsByEmployeeIdAsync(id);

                await _employeeRepository.DeleteAsync(employee);

                transactionScope.Complete();
            }
        }

        public async Task<bool> ExistAsync(Expression<Func<Employee, bool>> predicate)
        {
            return await _employeeRepository.ExistAsync(predicate);
        }

        public async Task UpdateLastSeenAsync(string deviceId)
        {
            var device = await _deviceService.GetVaultByIdAsync(deviceId);
            var employee = await _employeeRepository.GetByIdAsync(device.EmployeeId);

            if (employee != null)
            {
                employee.LastSeen = DateTime.UtcNow;
                await _employeeRepository.UpdateOnlyPropAsync(employee, new string[] { "LastSeen" });
            }
        }

        #endregion

        #region HardwareVault

        public async Task AddDeviceAsync(string employeeId, string[] vaults)
        {
            if (employeeId == null)
                throw new ArgumentNullException(nameof(employeeId));

            if (vaults == null)
                throw new ArgumentNullException(nameof(vaults));

            _dataProtectionService.Validate();

            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
                throw new Exception("Employee not found");

            if (employee.HardwareVaults?.Count() > 0)
                throw new Exception("More than one vault cannot be added to an employee");

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var vaultId in vaults)
                {
                    var vault = await _deviceService.GetVaultByIdAsync(vaultId);

                    if (vault == null)
                        throw new Exception($"Vault {vault} not found");

                    if (vault.Status != VaultStatus.Ready)
                        throw new Exception($"Vault {vaultId} in a status that does not allow to reserve.");

                    vault.EmployeeId = employeeId;
                    vault.Status = VaultStatus.Reserved;

                    var masterPassword = GenerateMasterPassword();

                    await _deviceService.UpdateOnlyPropAsync(vault, new string[] { nameof(HardwareVault.EmployeeId), nameof(HardwareVault.Status) });
                    await _deviceService.GenerateVaultActivationAsync(vaultId);
                    await _deviceTaskService.AddLinkAsync(vault.Id, _dataProtectionService.Encrypt(masterPassword));
                }

                transactionScope.Complete();
            }
        }

        public async Task RemoveDeviceAsync(string employeeId, string vaultId, VaultStatusReason reason)
        {
            if (employeeId == null)
                throw new ArgumentNullException(nameof(employeeId));

            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            _dataProtectionService.Validate();

            var vault = await _deviceService.GetVaultByIdAsync(vaultId);
            if (vault == null)
                throw new Exception($"Vault {vaultId} not found");

            if (vault.EmployeeId != employeeId)
                throw new Exception($"Vault {vaultId} not linked to this employee");

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _deviceTaskService.RemoveAllTasksAsync(vaultId);
                await _workstationService.RemoveAllProximityAsync(vaultId);

                vault.EmployeeId = null;

                if (vault.MasterPassword == null)
                {
                    vault.Status = VaultStatus.Ready;
                    await _deviceService.ChangeVaultActivationStatusAsync(vaultId, HardwareVaultActivationStatus.Canceled);
                }
                else
                {
                    vault.Status = VaultStatus.Deactivated;
                    vault.StatusReason = reason;
                    await _accountService.RemoveAllAccountsAsync(employeeId);
                }

                await _deviceService.UpdateOnlyPropAsync(vault, new string[] { nameof(HardwareVault.EmployeeId), nameof(HardwareVault.Status) });

                transactionScope.Complete();
            }
        }

        #endregion

        #region SamlIdp

        //public async Task CreateSamlIdpAccountAsync(string email, string password, string hesUrl, string deviceId)
        //{
        //    _dataProtectionService.Validate();

        //    var employee = await _employeeRepository.Query().FirstOrDefaultAsync(e => e.Email == email);
        //    if (employee == null)
        //    {
        //        throw new ArgumentNullException(nameof(employee));
        //    }

        //    var device = await _deviceService.GetDeviceByIdAsync(deviceId);
        //    if (device == null)
        //    {
        //        throw new ArgumentNullException(nameof(device));
        //    }

        //    var samlIdP = await _samlIdentityProviderService.GetByIdAsync(SamlIdentityProvider.PrimaryKey);

        //    // Create account
        //    var deviceAccountId = Guid.NewGuid().ToString();
        //    var deviceAccount = new Account
        //    {
        //        Id = deviceAccountId,
        //        Name = SamlIdentityProvider.DeviceAccountName,
        //        Urls = $"{samlIdP.Url};{hesUrl}",
        //        Apps = null,
        //        Login = email,
        //        Type = AccountType.Personal,
        //        CreatedAt = DateTime.UtcNow,
        //        PasswordUpdatedAt = DateTime.UtcNow,
        //        OtpUpdatedAt = null,
        //        EmployeeId = employee.Id,
        //        SharedAccountId = null
        //    };

        //    // Validate url
        //    deviceAccount.Urls = ValidationHepler.VerifyUrls(deviceAccount.Urls);

        //    // Create task
        //    var deviceTask = new DeviceTask
        //    {
        //        DeviceAccountId = deviceAccountId,
        //        OldName = deviceAccount.Name,
        //        OldUrls = deviceAccount.Urls,
        //        OldApps = deviceAccount.Apps,
        //        OldLogin = deviceAccount.Login,
        //        Password = _dataProtectionService.Encrypt(password),
        //        OtpSecret = null,
        //        CreatedAt = DateTime.UtcNow,
        //        Operation = TaskOperation.Create,
        //        DeviceId = device.Id
        //    };

        //    // Add account
        //    await _accountService.AddAsync(deviceAccount);

        //    try
        //    {
        //        // Add task
        //        await _deviceTaskService.AddTaskAsync(deviceTask);
        //    }
        //    catch (Exception)
        //    {
        //        // Remove account
        //        await _accountService.DeleteAsync(deviceAccount);
        //        throw;
        //    }
        //}

        //public async Task UpdatePasswordSamlIdpAccountAsync(string email, string password)
        //{
        //    _dataProtectionService.Validate();

        //    var employee = await _employeeRepository.Query().FirstOrDefaultAsync(e => e.Email == email);
        //    if (employee == null)
        //    {
        //        throw new ArgumentNullException(nameof(employee));
        //    }
        //    var account = await _accountService
        //     .Query()
        //     .Where(d => d.EmployeeId == employee.Id && d.Name == SamlIdentityProvider.DeviceAccountName)
        //     .FirstOrDefaultAsync();

        //    // Update Account
        //    account.UpdatedAt = DateTime.UtcNow;
        //    await _accountService.UpdateOnlyPropAsync(account, new string[] { nameof(Account.UpdatedAt) });

        //    // Create Device Task
        //    try
        //    {
        //        await _deviceTaskService.AddTaskAsync(new DeviceTask
        //        {
        //            DeviceAccountId = account.Id,
        //            Password = _dataProtectionService.Encrypt(password),
        //            CreatedAt = DateTime.UtcNow,
        //            Operation = TaskOperation.Update,
        //            DeviceId = account.DeviceId
        //        });
        //    }
        //    catch (Exception)
        //    {
        //        account.Status = AccountStatus.Error;
        //        await _accountService.UpdateOnlyPropAsync(account, properties);
        //        throw;
        //    }
        //}

        //public async Task UpdateOtpSamlIdpAccountAsync(string email, string otp)
        //{
        //    if (email == null)
        //    {
        //        throw new ArgumentNullException(nameof(email));
        //    }
        //    if (otp == null)
        //    {
        //        throw new ArgumentNullException(nameof(otp));
        //    }

        //    ValidationHepler.VerifyOtpSecret(otp);

        //    _dataProtectionService.Validate();

        //    var employee = await _employeeRepository.Query().FirstOrDefaultAsync(e => e.Email == email);
        //    if (employee == null)
        //    {
        //        throw new ArgumentNullException(nameof(employee));
        //    }
        //    var deviceAccount = await _accountService
        //     .Query()
        //     .Where(d => d.EmployeeId == employee.Id && d.Name == SamlIdentityProvider.DeviceAccountName)
        //     .FirstOrDefaultAsync();

        //    var task = await _deviceTaskService
        //        .Query()
        //        .AsNoTracking()
        //        .Where(d => d.DeviceAccountId == deviceAccount.Id && _dataProtectionService.Decrypt(d.OtpSecret) == otp)
        //        .FirstOrDefaultAsync();

        //    if (task != null)
        //    {
        //        return;
        //    }

        //    // Update Device Account
        //    deviceAccount.Status = AccountStatus.Updating;
        //    deviceAccount.UpdatedAt = DateTime.UtcNow;
        //    string[] properties = { "Status", "UpdatedAt" };
        //    await _accountService.UpdateOnlyPropAsync(deviceAccount, properties);

        //    // Create Device Task
        //    try
        //    {
        //        await _deviceTaskService.AddTaskAsync(new DeviceTask
        //        {
        //            DeviceAccountId = deviceAccount.Id,
        //            OtpSecret = _dataProtectionService.Encrypt(otp),
        //            CreatedAt = DateTime.UtcNow,
        //            Operation = TaskOperation.Update,
        //            DeviceId = deviceAccount.DeviceId
        //        });
        //    }
        //    catch (Exception)
        //    {
        //        deviceAccount.Status = AccountStatus.Error;
        //        await _accountService.UpdateOnlyPropAsync(deviceAccount, properties);
        //        throw;
        //    }
        //}

        //public async Task<IList<string>> UpdateUrlSamlIdpAccountAsync(string hesUrl)
        //{
        //    _dataProtectionService.Validate();

        //    var deviceAccounts = await _accountService
        //     .Query()
        //     .Where(d => d.Name == SamlIdentityProvider.DeviceAccountName && d.Deleted == false)
        //     .ToListAsync();

        //    var samlIdP = await _samlIdentityProviderService.GetByIdAsync(SamlIdentityProvider.PrimaryKey);
        //    var validUrls = ValidationHepler.VerifyUrls($"{samlIdP.Url};{hesUrl}");

        //    foreach (var account in deviceAccounts)
        //    {
        //        // Update Device Account
        //        account.Status = AccountStatus.Updating;
        //        account.UpdatedAt = DateTime.UtcNow;
        //        string[] properties = { "Status", "UpdatedAt" };
        //        await _accountService.UpdateOnlyPropAsync(account, properties);

        //        // Create Device Task
        //        try
        //        {
        //            await _deviceTaskService.AddTaskAsync(new DeviceTask
        //            {
        //                DeviceAccountId = account.Id,
        //                OldUrls = validUrls,
        //                CreatedAt = DateTime.UtcNow,
        //                Operation = TaskOperation.Update,
        //                DeviceId = account.DeviceId
        //            });
        //        }
        //        catch (Exception)
        //        {
        //            account.Status = AccountStatus.Error;
        //            await _accountService.UpdateOnlyPropAsync(account, properties);
        //            throw;
        //        }
        //    }

        //    return deviceAccounts.Select(s => s.Id).ToList();
        //}

        //public async Task DeleteSamlIdpAccountAsync(string employeeId)
        //{
        //    if (employeeId == null)
        //    {
        //        throw new ArgumentNullException(nameof(employeeId));
        //    }

        //    var employee = await _employeeRepository.GetByIdAsync(employeeId);
        //    if (employee == null)
        //    {
        //        throw new Exception("Employee not found.");
        //    }

        //    var account = await _accountService
        //        .Query()
        //        .Where(d => d.EmployeeId == employeeId && d.Name == SamlIdentityProvider.DeviceAccountName)
        //        .FirstOrDefaultAsync();

        //    if (account != null)
        //    {
        //        await DeleteAccountAsync(account.Id);
        //    }
        //}

        #endregion

        #region Account

        public async Task<Account> GetAccountByIdAsync(string accountId)
        {
            return await _accountService
                .Query()
                .Include(x => x.Employee.HardwareVaults)
                .Include(x => x.SharedAccount)
                .FirstOrDefaultAsync(x => x.Id == accountId);
        }

        public async Task<List<Account>> GetAccountsByEmployeeIdAsync(string employeeId)
        {
            return await _accountService
                .Query()
                .Include(x => x.Employee.HardwareVaults)
                .Include(x => x.SharedAccount)
                .Where(x => x.EmployeeId == employeeId && x.Deleted == false/* &&*/
                            /*x.Name != SamlIdentityProvider.DeviceAccountName*/)
                .ToListAsync();
        }

        public async Task<Account> CreatePersonalAccountAsync(Account account, AccountPassword accountPassword)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            if (accountPassword == null)
                throw new ArgumentNullException(nameof(accountPassword));

            _dataProtectionService.Validate();

            var employee = await GetEmployeeByIdAsync(account.EmployeeId);
            if (employee.HardwareVaults.Count == 0)
                throw new Exception("Employee has no device");

            var exist = await _accountService.ExistAsync(x => x.EmployeeId == account.EmployeeId &&
                                                         x.Name == account.Name &&
                                                         x.Login == account.Login &&
                                                         x.Deleted == false);
            if (exist)
                throw new Exception("An account with the same name and login exist.");

            account.Id = Guid.NewGuid().ToString();
            account.Type = AccountType.Personal;
            account.CreatedAt = DateTime.UtcNow;
            account.PasswordUpdatedAt = DateTime.UtcNow;
            account.OtpUpdatedAt = ValidationHepler.VerifyOtpSecret(accountPassword.OtpSecret) != null ? new DateTime?(DateTime.UtcNow) : null;
            account.Urls = ValidationHepler.VerifyUrls(account.Urls);

            Account createdAccount;
            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();

            foreach (var device in employee.HardwareVaults)
            {
                tasks.Add(new HardwareVaultTask
                {
                    Password = _dataProtectionService.Encrypt(accountPassword.Password),
                    OtpSecret = _dataProtectionService.Encrypt(accountPassword.OtpSecret),
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Create,
                    HardwareVaultId = device.Id,
                    AccountId = account.Id
                });
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                createdAccount = await _accountService.AddAsync(account);
                await _deviceTaskService.AddRangeTasksAsync(tasks);
                await _deviceService.UpdateNeedSyncAsync(employee.HardwareVaults, true);
                await SetAsWorkstationIfEmptyAsync(createdAccount.EmployeeId, createdAccount.Id);

                transactionScope.Complete();
            }

            return createdAccount;
        }

        public async Task<Account> CreateWorkstationAccountAsync(WorkstationAccount workstationAccount, string employeeId)
        {
            if (workstationAccount == null)
                throw new ArgumentNullException(nameof(workstationAccount));

            if (employeeId == null)
                throw new ArgumentNullException(nameof(employeeId));

            var deviceAccount = new Account()
            {
                Name = workstationAccount.Name,
                EmployeeId = employeeId,
                Kind = AccountKind.Workstation
            };

            switch (workstationAccount.AccountType)
            {
                case WorkstationAccountType.Local:
                    deviceAccount.Login = $".\\{workstationAccount.Login}";
                    break;
                case WorkstationAccountType.Domain:
                    deviceAccount.Login = $"{workstationAccount.Domain}\\{workstationAccount.Login}";
                    break;
                case WorkstationAccountType.Microsoft:
                    deviceAccount.Login = $"@\\{workstationAccount.Login}";
                    break;
                case WorkstationAccountType.AzureAD:
                    deviceAccount.Login = $"AzureAD\\{workstationAccount.Login}";
                    break;
            }

            return await CreatePersonalAccountAsync(deviceAccount, new AccountPassword() { Password = workstationAccount.Password });
        }

        public async Task SetAsWorkstationAccountAsync(string employeeId, string accountId)
        {
            if (employeeId == null)
            {
                throw new ArgumentNullException(nameof(employeeId));
            }

            if (accountId == null)
            {
                throw new ArgumentNullException(nameof(accountId));
            }

            var employee = await GetEmployeeByIdAsync(employeeId);
            if (employee == null)
                throw new Exception($"Employee not found");

            if (employee.HardwareVaults.Count == 0)
                throw new Exception("Employee has no device");

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Update employee
                employee.PrimaryAccountId = accountId;
                await _employeeRepository.UpdateOnlyPropAsync(employee, new string[] { nameof(Employee.PrimaryAccountId) });

                // Create tasks for devices
                foreach (var device in employee.HardwareVaults)
                {
                    await _deviceTaskService.AddPrimaryAsync(device.Id, accountId);
                    await _deviceService.UpdateNeedSyncAsync(device, true);
                }

                transactionScope.Complete();
            }
        }

        private async Task SetAsWorkstationIfEmptyAsync(string employeeId, string accountId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);

            if (employee.PrimaryAccountId == null)
            {
                employee.PrimaryAccountId = accountId;
                await _employeeRepository.UpdateOnlyPropAsync(employee, new string[] { nameof(Employee.PrimaryAccountId) });
            }
        }

        public async Task EditPersonalAccountAsync(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            _dataProtectionService.Validate();

            var employee = await GetEmployeeByIdAsync(account.EmployeeId);
            if (employee.HardwareVaults.Count == 0)
                throw new Exception("Employee has no device");

            var exist = await _accountService.ExistAsync(x => x.Name == account.Name && x.Login == account.Login && x.Deleted == false && x.Id != account.Id);
            if (exist)
                throw new Exception("An account with the same name and login exist.");

            account.Urls = ValidationHepler.VerifyUrls(account.Urls);
            account.UpdatedAt = DateTime.UtcNow;
            string[] properties = { nameof(Account.Name), nameof(Account.Login), nameof(Account.Urls), nameof(Account.Apps), nameof(Account.UpdatedAt) };

            // Create tasks  
            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();
            foreach (var device in employee.HardwareVaults)
            {
                tasks.Add(new HardwareVaultTask
                {
                    Operation = TaskOperation.Update,
                    CreatedAt = DateTime.UtcNow,
                    HardwareVaultId = device.Id,
                    AccountId = account.Id
                });
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _accountService.UpdateOnlyPropAsync(account, properties);
                await _deviceTaskService.AddRangeTasksAsync(tasks);
                await _deviceService.UpdateNeedSyncAsync(employee.HardwareVaults, true);
                transactionScope.Complete();
            }
        }

        public async Task EditPersonalAccountPwdAsync(Account account, AccountPassword accountPassword)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            if (accountPassword == null)
                throw new ArgumentNullException(nameof(accountPassword));

            _dataProtectionService.Validate();

            var employee = await GetEmployeeByIdAsync(account.EmployeeId);
            if (employee.HardwareVaults.Count == 0)
                throw new Exception("Employee has no device");

            // Update account
            account.UpdatedAt = DateTime.UtcNow;
            account.PasswordUpdatedAt = DateTime.UtcNow;
            string[] properties = { nameof(Account.UpdatedAt), nameof(Account.PasswordUpdatedAt) };

            // Create tasks
            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();
            foreach (var device in employee.HardwareVaults)
            {
                tasks.Add(new HardwareVaultTask
                {
                    Password = _dataProtectionService.Encrypt(accountPassword.Password),
                    Operation = TaskOperation.Update,
                    CreatedAt = DateTime.UtcNow,
                    HardwareVaultId = device.Id,
                    AccountId = account.Id
                });
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _accountService.UpdateOnlyPropAsync(account, properties);
                await _deviceTaskService.AddRangeTasksAsync(tasks);
                await _deviceService.UpdateNeedSyncAsync(employee.HardwareVaults, true);
                transactionScope.Complete();
            }
        }

        public async Task EditPersonalAccountOtpAsync(Account account, AccountPassword accountPassword)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            if (accountPassword == null)
                throw new ArgumentNullException(nameof(accountPassword));

            _dataProtectionService.Validate();

            var employee = await GetEmployeeByIdAsync(account.EmployeeId);
            if (employee.HardwareVaults.Count == 0)
                throw new Exception("Employee has no device");

            // Update account
            account.UpdatedAt = DateTime.UtcNow;
            account.OtpUpdatedAt = ValidationHepler.VerifyOtpSecret(accountPassword.OtpSecret) == null ? null : (DateTime?)DateTime.UtcNow;
            string[] properties = { nameof(Account.UpdatedAt), nameof(Account.OtpUpdatedAt) };

            // Create tasks
            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();
            foreach (var device in employee.HardwareVaults)
            {
                tasks.Add(new HardwareVaultTask
                {
                    OtpSecret = _dataProtectionService.Encrypt(accountPassword.OtpSecret ?? string.Empty),
                    Operation = TaskOperation.Update,
                    CreatedAt = DateTime.UtcNow,
                    HardwareVaultId = device.Id,
                    AccountId = account.Id
                });
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _accountService.UpdateOnlyPropAsync(account, properties);
                await _deviceTaskService.AddRangeTasksAsync(tasks);
                await _deviceService.UpdateNeedSyncAsync(employee.HardwareVaults, true);
                transactionScope.Complete();
            }
        }

        public async Task<Account> AddSharedAccountAsync(string employeeId, string sharedAccountId)
        {
            if (employeeId == null)
                throw new ArgumentNullException(nameof(employeeId));

            if (sharedAccountId == null)
                throw new ArgumentNullException(nameof(sharedAccountId));

            _dataProtectionService.Validate();

            var employee = await GetEmployeeByIdAsync(employeeId);
            if (employee.HardwareVaults.Count == 0)
                throw new Exception("Employee has no device");

            var sharedAccount = await _sharedAccountService.GetByIdAsync(sharedAccountId);
            if (sharedAccount == null)
                throw new Exception("Shared Account not found");

            var exist = await _accountService.ExistAsync(x => x.EmployeeId == employeeId &&
                                                         x.Name == sharedAccount.Name &&
                                                         x.Login == sharedAccount.Login &&
                                                         x.Deleted == false);
            if (exist)
                throw new Exception("An account with the same name and login exists");

            // Create account
            var account = new Account
            {
                Id = Guid.NewGuid().ToString(),
                Name = sharedAccount.Name,
                Urls = sharedAccount.Urls,
                Apps = sharedAccount.Apps,
                Login = sharedAccount.Login,
                Type = AccountType.Shared,
                CreatedAt = DateTime.UtcNow,
                PasswordUpdatedAt = DateTime.UtcNow,
                OtpUpdatedAt = sharedAccount.OtpSecret != null ? new DateTime?(DateTime.UtcNow) : null,
                EmployeeId = employeeId,
                SharedAccountId = sharedAccountId
            };

            Account createdAccount;
            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();

            foreach (var device in employee.HardwareVaults)
            {
                tasks.Add(new HardwareVaultTask
                {
                    Password = sharedAccount.Password,
                    OtpSecret = sharedAccount.OtpSecret,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Create,
                    HardwareVaultId = device.Id,
                    AccountId = account.Id
                });
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                createdAccount = await _accountService.AddAsync(account);
                await _deviceTaskService.AddRangeTasksAsync(tasks);
                await _deviceService.UpdateNeedSyncAsync(employee.HardwareVaults, true);
                await SetAsWorkstationIfEmptyAsync(createdAccount.EmployeeId, createdAccount.Id);

                transactionScope.Complete();
            }

            return createdAccount;
        }

        public async Task<Account> DeleteAccountAsync(string accountId)
        {
            if (accountId == null)
                throw new ArgumentNullException(nameof(accountId));

            _dataProtectionService.Validate();

            var account = await GetAccountByIdAsync(accountId);
            if (account == null)
                throw new NotFoundException("Account not found");

            var employee = await GetEmployeeByIdAsync(account.EmployeeId);
            if (employee.HardwareVaults.Count == 0)
                throw new Exception("Employee has no device");

            var isPrimary = employee.PrimaryAccountId == accountId;
            if (isPrimary)
                employee.PrimaryAccountId = null;

            account.Deleted = true;
            account.UpdatedAt = DateTime.UtcNow;
            string[] properties = { nameof(Account.Deleted), nameof(Account.UpdatedAt) };

            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();

            foreach (var device in employee.HardwareVaults)
            {
                tasks.Add(new HardwareVaultTask
                {
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Delete,
                    HardwareVaultId = device.Id,
                    AccountId = account.Id,
                });
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                if (isPrimary)
                    await _employeeRepository.UpdateOnlyPropAsync(employee, new string[] { nameof(Employee.PrimaryAccountId) });

                await _accountService.UpdateOnlyPropAsync(account, new string[] { nameof(Account.Deleted), nameof(Account.UpdatedAt) });
                await _deviceTaskService.AddRangeTasksAsync(tasks);
                await _deviceService.UpdateNeedSyncAsync(employee.HardwareVaults, true);
                transactionScope.Complete();
            }

            return account;
        }

        private string GenerateMasterPassword()
        {
            var buf = AesCryptoHelper.CreateRandomBuf(32);
            var pass = ConvertUtils.ByteArrayToHexString(buf);
            return pass;
        }

        #endregion

        //public async Task HandlingMasterPasswordErrorAsync(string deviceId)
        //{
        //    using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //    {
        //        await _deviceTaskService.RemoveAllTasksAsync(deviceId);
        //        await _accountService.RemoveAllAccountsAsync(deviceId);
        //        await _workstationService.RemoveAllProximityAsync(deviceId);
        //        await _deviceService.RemoveEmployeeAsync(deviceId);
        //        await _licenseService.DiscardLicenseAppliedAsync(deviceId);
        //        transactionScope.Complete();
        //    }
        //}
    }
}
