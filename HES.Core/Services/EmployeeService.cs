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

namespace HES.Core.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IAsyncRepository<Employee> _employeeRepository;
        private readonly IDeviceService _deviceService;
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly IDeviceAccountService _deviceAccountService;
        private readonly ISharedAccountService _sharedAccountService;
        private readonly IWorkstationService _workstationService;
        private readonly ILicenseService _licenseService;
        private readonly IAsyncRepository<WorkstationEvent> _workstationEventRepository;
        private readonly IAsyncRepository<WorkstationSession> _workstationSessionRepository;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly ISamlIdentityProviderService _samlIdentityProviderService;

        public EmployeeService(IAsyncRepository<Employee> employeeRepository,
                               IDeviceService deviceService,
                               IDeviceTaskService deviceTaskService,
                               IDeviceAccountService deviceAccountService,
                               ISharedAccountService sharedAccountService,
                               IWorkstationService workstationService,
                               ILicenseService licenseService,
                               IAsyncRepository<WorkstationEvent> workstationEventRepository,
                               IAsyncRepository<WorkstationSession> workstationSessionRepository,
                               IDataProtectionService dataProtectionService,
                               ISamlIdentityProviderService samlIdentityProviderService)
        {
            _employeeRepository = employeeRepository;
            _deviceService = deviceService;
            _deviceTaskService = deviceTaskService;
            _deviceAccountService = deviceAccountService;
            _sharedAccountService = sharedAccountService;
            _workstationService = workstationService;
            _licenseService = licenseService;
            _workstationEventRepository = workstationEventRepository;
            _workstationSessionRepository = workstationSessionRepository;
            _dataProtectionService = dataProtectionService;
            _samlIdentityProviderService = samlIdentityProviderService;
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
                .Include(e => e.Devices)
                .ThenInclude(e => e.DeviceAccessProfile)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            return await _employeeRepository
                .Query()
                .Include(e => e.Department.Company)
                .Include(e => e.Position)
                .Include(e => e.Devices)
                .ToListAsync();
        }

        public async Task<List<Employee>> GetFilteredEmployeesAsync(EmployeeFilter employeeFilter)
        {
            var filter = _employeeRepository
                .Query()
                .Include(e => e.Department.Company)
                .Include(e => e.Position)
                .Include(e => e.Devices)
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
                filter = filter.Where(w => w.Devices.Count() == employeeFilter.DevicesCount);
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
                throw new Exception($"{employee.FirstName} {employee.LastName} already in use.");
            }

            return await _employeeRepository.AddAsync(employee);
        }

        public async Task EditEmployeeAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

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
                .DeviceQuery()
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
                await _deviceAccountService.RemoveAllAccountsByEmployeeIdAsync(id);

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
            var device = await _deviceService.GetDeviceByIdAsync(deviceId);
            var employee = await _employeeRepository.GetByIdAsync(device.EmployeeId);

            if (employee != null)
            {
                employee.LastSeen = DateTime.UtcNow;
                await _employeeRepository.UpdateOnlyPropAsync(employee, new string[] { "LastSeen" });
            }
        }

        #endregion

        #region Device

        public async Task AddDeviceAsync(string employeeId, string[] devices)
        {
            if (employeeId == null)
            {
                throw new ArgumentNullException(nameof(employeeId));
            }

            if (devices == null)
            {
                throw new ArgumentNullException(nameof(devices));
            }

            _dataProtectionService.Validate();

            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                throw new Exception("Employee not found");
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var deviceId in devices)
                {
                    var device = await _deviceService.GetDeviceByIdAsync(deviceId);
                    if (device != null)
                    {
                        if (device.MasterPassword != null)
                        {
                            throw new Exception($"{deviceId} already linked to employee.");
                        }

                        if (device.State == DeviceState.WaitingForWipe)
                        {
                            throw new Exception($"{deviceId} waiting for wipe.");
                        }

                        if (device.EmployeeId == null)
                        {
                            var masterPassword = GenerateMasterPassword();

                            device.EmployeeId = employeeId;

                            await _deviceService.UpdateOnlyPropAsync(device, new string[] { "EmployeeId" });
                            await _deviceTaskService.AddLinkAsync(device.Id, _dataProtectionService.Encrypt(masterPassword));

                        }
                    }
                }

                transactionScope.Complete();
            }
        }

        public async Task RemoveDeviceAsync(string employeeId, string deviceId)
        {
            if (employeeId == null)
            {
                throw new ArgumentNullException(nameof(employeeId));
            }

            if (deviceId == null)
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            _dataProtectionService.Validate();

            var device = await _deviceService.GetDeviceByIdAsync(deviceId);
            if (device == null)
            {
                throw new Exception($"Device not found");
            }

            if (device.EmployeeId != employeeId)
            {
                throw new Exception($"Device {deviceId} not linked to employee");
            }


            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Remove all tasks
                await _deviceTaskService.RemoveAllTasksAsync(deviceId);
                // Remove all accounts
                await _deviceAccountService.RemoveAllAccountsAsync(deviceId);
                // Remove all proximity device
                await _workstationService.RemoveAllProximityAsync(deviceId);
                // Remove employee from device
                device.EmployeeId = null;
                device.PrimaryAccountId = null;
                device.AcceessProfileId = "default";

                if (device.MasterPassword != null)
                {
                    device.State = DeviceState.WaitingForWipe;
                    await _deviceTaskService.AddWipeAsync(device.Id, device.MasterPassword);
                }

                await _deviceService.UpdateOnlyPropAsync(device, new string[] { "EmployeeId", "PrimaryAccountId", "AcceessProfileId", "State" });

                transactionScope.Complete();
            }
        }

        #endregion

        #region SamlIdp

        public async Task CreateSamlIdpAccountAsync(string email, string password, string hesUrl, string deviceId)
        {
            _dataProtectionService.Validate();

            var employee = await _employeeRepository.Query().FirstOrDefaultAsync(e => e.Email == email);
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            var device = await _deviceService.GetDeviceByIdAsync(deviceId);
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            var samlIdP = await _samlIdentityProviderService.GetByIdAsync(SamlIdentityProvider.PrimaryKey);

            // Create account
            var deviceAccountId = Guid.NewGuid().ToString();
            var deviceAccount = new DeviceAccount
            {
                Id = deviceAccountId,
                Name = SamlIdentityProvider.DeviceAccountName,
                Urls = $"{samlIdP.Url};{hesUrl}",
                Apps = null,
                Login = email,
                Type = AccountType.Personal,
                Status = AccountStatus.Creating,
                CreatedAt = DateTime.UtcNow,
                PasswordUpdatedAt = DateTime.UtcNow,
                OtpUpdatedAt = null,
                EmployeeId = employee.Id,
                DeviceId = device.Id,
                SharedAccountId = null
            };

            // Validate url
            deviceAccount.Urls = ValidationHepler.VerifyUrls(deviceAccount.Urls);

            // Create task
            var deviceTask = new DeviceTask
            {
                DeviceAccountId = deviceAccountId,
                OldName = deviceAccount.Name,
                OldUrls = deviceAccount.Urls,
                OldApps = deviceAccount.Apps,
                OldLogin = deviceAccount.Login,
                Password = _dataProtectionService.Encrypt(password),
                OtpSecret = null,
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Create,
                DeviceId = device.Id
            };

            // Add account
            await _deviceAccountService.AddAsync(deviceAccount);

            try
            {
                // Add task
                await _deviceTaskService.AddTaskAsync(deviceTask);
            }
            catch (Exception)
            {
                // Remove account
                await _deviceAccountService.DeleteAsync(deviceAccount);
                throw;
            }
        }

        public async Task UpdatePasswordSamlIdpAccountAsync(string email, string password)
        {
            _dataProtectionService.Validate();

            var employee = await _employeeRepository.Query().FirstOrDefaultAsync(e => e.Email == email);
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }
            var deviceAccount = await _deviceAccountService
             .Query()
             .Where(d => d.EmployeeId == employee.Id && d.Name == SamlIdentityProvider.DeviceAccountName)
             .FirstOrDefaultAsync();

            // Update Device Account
            deviceAccount.Status = AccountStatus.Updating;
            deviceAccount.UpdatedAt = DateTime.UtcNow;
            string[] properties = { "Status", "UpdatedAt" };
            await _deviceAccountService.UpdateOnlyPropAsync(deviceAccount, properties);

            // Create Device Task
            try
            {
                await _deviceTaskService.AddTaskAsync(new DeviceTask
                {
                    DeviceAccountId = deviceAccount.Id,
                    Password = _dataProtectionService.Encrypt(password),
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Update,
                    DeviceId = deviceAccount.DeviceId
                });
            }
            catch (Exception)
            {
                deviceAccount.Status = AccountStatus.Error;
                await _deviceAccountService.UpdateOnlyPropAsync(deviceAccount, properties);
                throw;
            }
        }

        public async Task UpdateOtpSamlIdpAccountAsync(string email, string otp)
        {
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }
            if (otp == null)
            {
                throw new ArgumentNullException(nameof(otp));
            }

            ValidationHepler.VerifyOtpSecret(otp);

            _dataProtectionService.Validate();

            var employee = await _employeeRepository.Query().FirstOrDefaultAsync(e => e.Email == email);
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }
            var deviceAccount = await _deviceAccountService
             .Query()
             .Where(d => d.EmployeeId == employee.Id && d.Name == SamlIdentityProvider.DeviceAccountName)
             .FirstOrDefaultAsync();

            var task = await _deviceTaskService
                .Query()
                .AsNoTracking()
                .Where(d => d.DeviceAccountId == deviceAccount.Id && _dataProtectionService.Decrypt(d.OtpSecret) == otp)
                .FirstOrDefaultAsync();

            if (task != null)
            {
                return;
            }

            // Update Device Account
            deviceAccount.Status = AccountStatus.Updating;
            deviceAccount.UpdatedAt = DateTime.UtcNow;
            string[] properties = { "Status", "UpdatedAt" };
            await _deviceAccountService.UpdateOnlyPropAsync(deviceAccount, properties);

            // Create Device Task
            try
            {
                await _deviceTaskService.AddTaskAsync(new DeviceTask
                {
                    DeviceAccountId = deviceAccount.Id,
                    OtpSecret = _dataProtectionService.Encrypt(otp),
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Update,
                    DeviceId = deviceAccount.DeviceId
                });
            }
            catch (Exception)
            {
                deviceAccount.Status = AccountStatus.Error;
                await _deviceAccountService.UpdateOnlyPropAsync(deviceAccount, properties);
                throw;
            }
        }

        public async Task<IList<string>> UpdateUrlSamlIdpAccountAsync(string hesUrl)
        {
            _dataProtectionService.Validate();

            var deviceAccounts = await _deviceAccountService
             .Query()
             .Where(d => d.Name == SamlIdentityProvider.DeviceAccountName && d.Deleted == false)
             .ToListAsync();

            var samlIdP = await _samlIdentityProviderService.GetByIdAsync(SamlIdentityProvider.PrimaryKey);
            var validUrls = ValidationHepler.VerifyUrls($"{samlIdP.Url};{hesUrl}");

            foreach (var account in deviceAccounts)
            {
                // Update Device Account
                account.Status = AccountStatus.Updating;
                account.UpdatedAt = DateTime.UtcNow;
                string[] properties = { "Status", "UpdatedAt" };
                await _deviceAccountService.UpdateOnlyPropAsync(account, properties);

                // Create Device Task
                try
                {
                    await _deviceTaskService.AddTaskAsync(new DeviceTask
                    {
                        DeviceAccountId = account.Id,
                        OldUrls = validUrls,
                        CreatedAt = DateTime.UtcNow,
                        Operation = TaskOperation.Update,
                        DeviceId = account.DeviceId
                    });
                }
                catch (Exception)
                {
                    account.Status = AccountStatus.Error;
                    await _deviceAccountService.UpdateOnlyPropAsync(account, properties);
                    throw;
                }
            }

            return deviceAccounts.Select(s => s.Id).ToList();
        }

        public async Task DeleteSamlIdpAccountAsync(string employeeId)
        {
            if (employeeId == null)
            {
                throw new ArgumentNullException(nameof(employeeId));
            }

            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                throw new Exception("Employee not found.");
            }

            var account = await _deviceAccountService
                .Query()
                .Where(d => d.EmployeeId == employeeId && d.Name == SamlIdentityProvider.DeviceAccountName)
                .FirstOrDefaultAsync();

            if (account != null)
            {
                await DeleteAccountAsync(account.Id);
            }
        }

        #endregion

        #region Account
        public async Task<DeviceAccount> GetDeviceAccountByIdAsync(string deviceAccountId)
        {
            return await _deviceAccountService
                .Query()
                .Include(d => d.Device)
                .Include(d => d.Employee)
                .Include(d => d.SharedAccount)
                .Where(e => e.Id == deviceAccountId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<DeviceAccount>> GetDeviceAccountsByEmployeeIdAsync(string employeeId)
        {
            return await _deviceAccountService
                .Query()
                .Include(d => d.Device)
                .Include(d => d.Employee)
                .Include(d => d.SharedAccount)
                .Where(e => e.EmployeeId == employeeId &&
                            e.Deleted == false &&
                            e.Name != SamlIdentityProvider.DeviceAccountName)
                .ToListAsync();
        }

        public async Task SetAsWorkstationAccountAsync(string deviceId, string deviceAccountId)
        {
            if (deviceId == null)
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            if (deviceAccountId == null)
            {
                throw new ArgumentNullException(nameof(deviceAccountId));
            }

            var device = await _deviceService.GetDeviceByIdAsync(deviceId);
            if (device == null)
            {
                throw new Exception($"Device not found, ID: {deviceId}");
            }

            var currentPrimaryAccountId = device.PrimaryAccountId;

            var deviceAccount = await _deviceAccountService.GetByIdAsync(deviceAccountId);
            if (deviceAccount == null)
            {
                throw new Exception($"DeviceAccount not found, ID: {deviceAccountId}");
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                device.PrimaryAccountId = deviceAccountId;
                await _deviceService.UpdateOnlyPropAsync(device, new string[] { "PrimaryAccountId" });

                deviceAccount.Status = AccountStatus.Updating;
                deviceAccount.UpdatedAt = DateTime.UtcNow;
                await _deviceAccountService.UpdateOnlyPropAsync(deviceAccount, new string[] { "Status", "UpdatedAt" });

                // Add task
                await _deviceTaskService.AddPrimaryAsync(device.Id, currentPrimaryAccountId, deviceAccountId);

                transactionScope.Complete();
            }
        }

        private async Task SetAsWorkstationIfEmptyAsync(string deviceId, string deviceAccountId)
        {
            var device = await _deviceService.GetDeviceByIdAsync(deviceId);

            if (device.PrimaryAccountId == null)
            {
                device.PrimaryAccountId = deviceAccountId;
                await _deviceService.UpdateOnlyPropAsync(device, new string[] { "PrimaryAccountId" });
            }
        }

        public async Task<IList<DeviceAccount>> CreateWorkstationAccountAsync(WorkstationAccount workstationAccount, string employeeId, string deviceId)
        {
            if (workstationAccount == null)
            {
                throw new ArgumentNullException(nameof(workstationAccount));
            }
            if (employeeId == null)
            {
                throw new ArgumentNullException(nameof(employeeId));
            }
            if (deviceId == null)
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            var deviceAccount = new DeviceAccount()
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
            }

            return await CreatePersonalAccountAsync(deviceAccount, new AccountPassword() { Password = workstationAccount.Password }, new string[] { deviceId });
        }

        public async Task<IList<DeviceAccount>> CreatePersonalAccountAsync(DeviceAccount deviceAccount, AccountPassword accountPassword, string[] selectedDevices)
        {
            if (deviceAccount == null)
                throw new ArgumentNullException(nameof(deviceAccount));

            if (accountPassword == null)
                throw new ArgumentNullException(nameof(accountPassword));

            if (selectedDevices == null)
                throw new ArgumentNullException(nameof(selectedDevices));
                     
            ValidationHepler.VerifyOtpSecret(accountPassword.OtpSecret);

            _dataProtectionService.Validate();

            List<DeviceAccount> accounts = new List<DeviceAccount>();
            List<DeviceTask> tasks = new List<DeviceTask>();

            IList<DeviceAccount> deviceAccounts;

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var deviceId in selectedDevices)
                {
                    var device = await _deviceService.GetDeviceByIdAsync(deviceId);
                    if (device.EmployeeId != deviceAccount.EmployeeId)
                    {
                        throw new Exception("This device and this employee are not linked.");
                    }
                    
                    var exist = await _deviceAccountService
                        .Query()
                        .Where(s => s.Name == deviceAccount.Name)
                        .Where(s => s.Login == deviceAccount.Login)
                        .Where(s => s.Deleted == false)
                        .Where(s => s.DeviceId == deviceId)
                        .AnyAsync();

                    if (exist)
                        throw new Exception("An account with the same name and login exists.");

                    // Validate url
                    deviceAccount.Urls = ValidationHepler.VerifyUrls(deviceAccount.Urls);

                    // Create Device Account
                    var deviceAccountId = Guid.NewGuid().ToString();
                    accounts.Add(new DeviceAccount
                    {
                        Id = deviceAccountId,
                        Name = deviceAccount.Name,
                        Urls = deviceAccount.Urls,
                        Apps = deviceAccount.Apps,
                        Login = deviceAccount.Login,
                        Type = AccountType.Personal,
                        Status = AccountStatus.Creating,
                        CreatedAt = DateTime.UtcNow,
                        PasswordUpdatedAt = DateTime.UtcNow,
                        OtpUpdatedAt = accountPassword.OtpSecret != null ? new DateTime?(DateTime.UtcNow) : null,
                        Kind = deviceAccount.Kind,
                        EmployeeId = deviceAccount.EmployeeId,
                        DeviceId = deviceId,
                        SharedAccountId = null
                    });

                    // Create Device Task
                    tasks.Add(new DeviceTask
                    {
                        DeviceAccountId = deviceAccountId,
                        OldName = deviceAccount.Name,
                        OldUrls = deviceAccount.Urls,
                        OldApps = deviceAccount.Apps,
                        OldLogin = deviceAccount.Login,
                        Password = _dataProtectionService.Encrypt(accountPassword.Password),
                        OtpSecret = _dataProtectionService.Encrypt(accountPassword.OtpSecret),
                        CreatedAt = DateTime.UtcNow,
                        Operation = TaskOperation.Create,
                        DeviceId = deviceId
                    });

                    // Set primary account
                    await SetAsWorkstationIfEmptyAsync(deviceId, deviceAccountId);
                }

                deviceAccounts = await _deviceAccountService.AddRangeAsync(accounts);
                await _deviceTaskService.AddRangeTasksAsync(tasks);

                transactionScope.Complete();
            }

            return deviceAccounts;
        }

        public async Task EditPersonalAccountAsync(DeviceAccount deviceAccount)
        {
            if (deviceAccount == null)
            {
                throw new ArgumentNullException(nameof(deviceAccount));
            }

            _dataProtectionService.Validate();

            var exist = await _deviceAccountService
                .Query()
                .Where(s => s.Name == deviceAccount.Name)
                .Where(s => s.Login == deviceAccount.Login)
                .Where(s => s.Deleted == false)
                .Where(s => s.Id != deviceAccount.Id)
                .Where(s => s.DeviceId == deviceAccount.DeviceId)
                .AnyAsync();

            if (exist)
            {
                throw new Exception("An account with the same name and login exists.");
            }

            // Get current device account
            var currentDeviceAccount = await _deviceAccountService.Query().AsNoTracking().FirstOrDefaultAsync(d => d.Id == deviceAccount.Id);

            // Validate url
            deviceAccount.Urls = ValidationHepler.VerifyUrls(deviceAccount.Urls);

            // Update Device Account
            deviceAccount.Status = AccountStatus.Updating;
            deviceAccount.UpdatedAt = DateTime.UtcNow;
            string[] properties = { "Name", "Login", "Urls", "Apps", "Status", "UpdatedAt" };
            // Create Task  
            var task = new DeviceTask
            {
                OldName = currentDeviceAccount.Name,
                OldUrls = currentDeviceAccount.Urls,
                OldApps = currentDeviceAccount.Apps,
                OldLogin = currentDeviceAccount.Login,
                Operation = TaskOperation.Update,
                CreatedAt = DateTime.UtcNow,
                DeviceId = deviceAccount.DeviceId,
                DeviceAccountId = deviceAccount.Id
            };

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _deviceAccountService.UpdateOnlyPropAsync(deviceAccount, properties);
                await _deviceTaskService.AddTaskAsync(task);
                transactionScope.Complete();
            }
        }

        public async Task EditPersonalAccountPwdAsync(DeviceAccount deviceAccount, AccountPassword accountPassword)
        {
            if (deviceAccount == null)
            {
                throw new ArgumentNullException(nameof(deviceAccount));
            }

            if (accountPassword == null)
            {
                throw new ArgumentNullException(nameof(accountPassword));
            }

            _dataProtectionService.Validate();

            // Update Device Account
            deviceAccount.Status = AccountStatus.Updating;
            deviceAccount.UpdatedAt = DateTime.UtcNow;
            deviceAccount.PasswordUpdatedAt = DateTime.UtcNow;
            string[] properties = { "Status", "UpdatedAt", "PasswordUpdatedAt" };
            // Create Device Task
            var task = new DeviceTask
            {
                Password = _dataProtectionService.Encrypt(accountPassword.Password),
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Update,
                DeviceId = deviceAccount.DeviceId,
                DeviceAccountId = deviceAccount.Id
            };

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _deviceAccountService.UpdateOnlyPropAsync(deviceAccount, properties);
                await _deviceTaskService.AddTaskAsync(task);
                transactionScope.Complete();
            }
        }

        public async Task EditPersonalAccountOtpAsync(DeviceAccount deviceAccount, AccountPassword accountPassword)
        {
            if (deviceAccount == null)
            {
                throw new ArgumentNullException(nameof(deviceAccount));
            }

            if (accountPassword == null)
            {
                throw new ArgumentNullException(nameof(accountPassword));
            }

            _dataProtectionService.Validate();

            ValidationHepler.VerifyOtpSecret(accountPassword.OtpSecret);

            // Update Device Account
            deviceAccount.Status = AccountStatus.Updating;
            deviceAccount.UpdatedAt = DateTime.UtcNow;
            deviceAccount.OtpUpdatedAt = accountPassword.OtpSecret == null ? null : (DateTime?)DateTime.UtcNow;
            string[] properties = { "Status", "UpdatedAt", "OtpUpdatedAt" };
            // Create Device Task
            var task = new DeviceTask
            {
                OtpSecret = _dataProtectionService.Encrypt(accountPassword.OtpSecret ?? string.Empty),
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Update,
                DeviceId = deviceAccount.DeviceId,
                DeviceAccountId = deviceAccount.Id
            };

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _deviceAccountService.UpdateOnlyPropAsync(deviceAccount, properties);
                await _deviceTaskService.AddTaskAsync(task);
                transactionScope.Complete();
            }
        }

        public async Task<IList<DeviceAccount>> AddSharedAccountAsync(string employeeId, string sharedAccountId, string[] devicesIds)
        {
            if (employeeId == null)
                throw new ArgumentNullException(nameof(employeeId));

            if (sharedAccountId == null)
                throw new ArgumentNullException(nameof(sharedAccountId));

            if (devicesIds == null)
                throw new ArgumentNullException(nameof(devicesIds));

            _dataProtectionService.Validate();

            List<DeviceAccount> accounts = new List<DeviceAccount>();
            List<DeviceTask> tasks = new List<DeviceTask>();

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var deviceId in devicesIds)
                {
                    // Get Shared Account
                    var sharedAccount = await _sharedAccountService.GetByIdAsync(sharedAccountId);
                    if (sharedAccount == null)
                        throw new Exception("SharedAccount not found");

                    var exist = await _deviceAccountService
                        .Query()
                        .Where(s => s.Name == sharedAccount.Name)
                        .Where(s => s.Login == sharedAccount.Login)
                        .Where(s => s.Deleted == false)
                        .Where(d => d.DeviceId == deviceId)
                        .AnyAsync();

                    if (exist)
                        throw new Exception("An account with the same name and login exists");

                    // Create Device Account
                    var deviceAccountId = Guid.NewGuid().ToString();
                    accounts.Add(new DeviceAccount
                    {
                        Id = deviceAccountId,
                        Name = sharedAccount.Name,
                        Urls = sharedAccount.Urls,
                        Apps = sharedAccount.Apps,
                        Login = sharedAccount.Login,
                        Type = AccountType.Shared,
                        Status = AccountStatus.Creating,
                        CreatedAt = DateTime.UtcNow,
                        PasswordUpdatedAt = DateTime.UtcNow,
                        OtpUpdatedAt = sharedAccount.OtpSecret != null ? new DateTime?(DateTime.UtcNow) : null,
                        EmployeeId = employeeId,
                        DeviceId = deviceId,
                        SharedAccountId = sharedAccountId
                    });

                    // Create Device Task
                    tasks.Add(new DeviceTask
                    {
                        DeviceAccountId = deviceAccountId,
                        OldName = sharedAccount.Name,
                        OldUrls = sharedAccount.Urls,
                        OldApps = sharedAccount.Apps,
                        OldLogin = sharedAccount.Login,
                        Password = sharedAccount.Password,
                        OtpSecret = sharedAccount.OtpSecret,
                        CreatedAt = DateTime.UtcNow,
                        Operation = TaskOperation.Create,
                        DeviceId = deviceId
                    });

                    // Set primary account
                    await SetAsWorkstationIfEmptyAsync(deviceId, deviceAccountId);
                }

                await _deviceAccountService.AddRangeAsync(accounts);
                await _deviceTaskService.AddRangeTasksAsync(tasks);

                transactionScope.Complete();
            }
            return accounts;
        }

        public async Task<string> DeleteAccountAsync(string accountId)
        {
            _dataProtectionService.Validate();

            if (accountId == null)
                throw new ArgumentNullException(nameof(accountId));

            var deviceAccount = await _deviceAccountService.GetByIdAsync(accountId);
            if (deviceAccount == null)
                throw new Exception("Device account not found");

            if (deviceAccount.Status == AccountStatus.Creating)
            {
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    deviceAccount.Deleted = true;
                    await _deviceAccountService.UpdateOnlyPropAsync(deviceAccount, new string[] { "Deleted" });
                    var task = await _deviceTaskService.Query().FirstOrDefaultAsync(d => d.DeviceAccountId == deviceAccount.Id);
                    await _deviceTaskService.DeleteTaskAsync(task);
                    transactionScope.Complete();
                }
                return deviceAccount.DeviceId;
            }

            // Update Device Account
            deviceAccount.Status = AccountStatus.Removing;
            deviceAccount.UpdatedAt = DateTime.UtcNow;
            string[] properties = { "Status", "UpdatedAt" };
            // Create Device Task
            var taskToDelete = new DeviceTask
            {
                DeviceAccountId = accountId,
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Delete,
                DeviceId = deviceAccount.DeviceId
            };

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _deviceAccountService.UpdateOnlyPropAsync(deviceAccount, properties);
                await _deviceTaskService.AddTaskAsync(taskToDelete);
                transactionScope.Complete();
            }
            return deviceAccount.DeviceId;
        }

        public async Task<DeviceAccount> GetLastChangedAccountAsync(string deviceId)
        {
            if (deviceId == null)
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            return await _deviceTaskService.GetLastChangedAccountAsync(deviceId);
        }

        public async Task UndoChangesAsync(string deviceId)
        {
            if (deviceId == null)
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            _dataProtectionService.Validate();

            await _deviceTaskService.UndoLastTaskAsync(deviceId);
        }

        private string GenerateMasterPassword()
        {
            var buf = AesCryptoHelper.CreateRandomBuf(32);
            var pass = ConvertUtils.ByteArrayToHexString(buf);
            return pass;
        }

        #endregion

        public async Task HandlingMasterPasswordErrorAsync(string deviceId)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _deviceTaskService.RemoveAllTasksAsync(deviceId);
                await _deviceAccountService.RemoveAllAccountsAsync(deviceId);
                await _workstationService.RemoveAllProximityAsync(deviceId);
                await _deviceService.RemoveEmployeeAsync(deviceId);
                await _licenseService.DiscardLicenseAppliedAsync(deviceId);
                transactionScope.Complete();
            }
        }
    }
}