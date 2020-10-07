﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Employees;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Accounts;
using HES.Core.Utilities;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Security;
using Hideez.SDK.Communication.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Core.Services
{
    public class EmployeeService : IEmployeeService, IDisposable
    {
        private readonly IAsyncRepository<Employee> _employeeRepository;
        private readonly IHardwareVaultService _hardwareVaultService;
        private readonly IHardwareVaultTaskService _hardwareVaultTaskService;
        private readonly ISoftwareVaultService _softwareVaultService;
        private readonly IAccountService _accountService;
        private readonly ISharedAccountService _sharedAccountService;
        private readonly IWorkstationService _workstationService;
        private readonly IDataProtectionService _dataProtectionService;

        public EmployeeService(IAsyncRepository<Employee> employeeRepository,
                               IHardwareVaultService hardwareVaultService,
                               IHardwareVaultTaskService hardwareVaultTaskService,
                               ISoftwareVaultService softwareVaultService,
                               IAccountService accountService,
                               ISharedAccountService sharedAccountService,
                               IWorkstationService workstationService,
                               IDataProtectionService dataProtectionService)
        {
            _employeeRepository = employeeRepository;
            _hardwareVaultService = hardwareVaultService;
            _hardwareVaultTaskService = hardwareVaultTaskService;
            _softwareVaultService = softwareVaultService;
            _accountService = accountService;
            _sharedAccountService = sharedAccountService;
            _workstationService = workstationService;
            _dataProtectionService = dataProtectionService;
        }

        #region Employee

        public IQueryable<Employee> EmployeeQuery()
        {
            return _employeeRepository.Query();
        }

        public async Task<Employee> GetEmployeeByIdAsync(string id, bool asNoTracking = false, bool byActiveDirectoryGuid = false)
        {
            var query = _employeeRepository
                .Query()
                .Include(e => e.Department.Company)
                .Include(e => e.Position)
                .Include(e => e.SoftwareVaults)
                .Include(e => e.SoftwareVaultInvitations)
                .Include(e => e.HardwareVaults)
                .ThenInclude(e => e.HardwareVaultProfile)
                .Include(e => e.Accounts)
                .AsQueryable();

            if (asNoTracking)
                query = query.AsNoTracking();

            if (!byActiveDirectoryGuid)
            {
                return await query.FirstOrDefaultAsync(e => e.Id == id);
            }
            else
            {
                return await query.FirstOrDefaultAsync(e => e.ActiveDirectoryGuid == id);
            }
        }

        public Task UnchangedEmployeeAsync(Employee employee)
        {
            return _employeeRepository.UnchangedAsync(employee);
        }

        public async Task<List<Employee>> GetEmployeesAsync(DataLoadingOptions<EmployeeFilter> dataLoadingOptions)
        {
            var query = _employeeRepository
                .Query()
                .Include(x => x.Department.Company)
                .Include(x => x.Position)
                .Include(x => x.HardwareVaults)
                .Include(x => x.SoftwareVaults)
                .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Employee != null)
                {
                    query = query.Where(x => (x.FirstName + " " + x.LastName).Contains(dataLoadingOptions.Filter.Employee, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Email != null)
                {
                    query = query.Where(w => w.Email.Contains(dataLoadingOptions.Filter.Email, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.PhoneNumber != null)
                {
                    query = query.Where(w => w.PhoneNumber.Contains(dataLoadingOptions.Filter.PhoneNumber, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    query = query.Where(x => x.Department.Company.Name.Contains(dataLoadingOptions.Filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    query = query.Where(x => x.Department.Name.Contains(dataLoadingOptions.Filter.Department, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Position != null)
                {
                    query = query.Where(x => x.Position.Name.Contains(dataLoadingOptions.Filter.Position, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.LastSeenStartDate != null)
                {
                    query = query.Where(w => w.LastSeen >= dataLoadingOptions.Filter.LastSeenStartDate);
                }
                if (dataLoadingOptions.Filter.LastSeenEndDate != null)
                {
                    query = query.Where(x => x.LastSeen <= dataLoadingOptions.Filter.LastSeenEndDate);
                }
                if (dataLoadingOptions.Filter.VaultsCount != null)
                {
                    query = query.Where(x => (x.HardwareVaults.Count + x.SoftwareVaults.Count) == dataLoadingOptions.Filter.VaultsCount);
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => (x.FirstName + " " + x.LastName).Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Email.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.PhoneNumber.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Company.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Position.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    (x.HardwareVaults.Count + x.SoftwareVaults.Count).ToString().Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Sort Direction
            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(Employee.FullName):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.FirstName).ThenBy(x => x.LastName) : query.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.LastName);
                    break;
                case nameof(Employee.Email):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Email) : query.OrderByDescending(x => x.Email);
                    break;
                case nameof(Employee.PhoneNumber):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.PhoneNumber) : query.OrderByDescending(x => x.PhoneNumber);
                    break;
                case nameof(Employee.Department.Company):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Department.Company.Name) : query.OrderByDescending(x => x.Department.Company.Name);
                    break;
                case nameof(Employee.Department):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Department.Name) : query.OrderByDescending(x => x.Department.Name);
                    break;
                case nameof(Employee.Position):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Position.Name) : query.OrderByDescending(x => x.Position.Name);
                    break;
                case nameof(Employee.LastSeen):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.LastSeen) : query.OrderByDescending(x => x.LastSeen);
                    break;
                case nameof(Employee.VaultsCount):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.HardwareVaults.Count).ThenBy(x => x.SoftwareVaults.Count) : query.OrderByDescending(x => x.HardwareVaults.Count).ThenByDescending(x => x.SoftwareVaults.Count);
                    break;
            }

            return await query.Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetEmployeesCountAsync(DataLoadingOptions<EmployeeFilter> dataLoadingOptions)
        {
            var query = _employeeRepository
            .Query()
            .Include(x => x.Department.Company)
            .Include(x => x.Position)
            .Include(x => x.HardwareVaults)
            .Include(x => x.SoftwareVaults)
            .AsQueryable();


            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Employee != null)
                {
                    query = query.Where(x => (x.FirstName + " " + x.LastName).Contains(dataLoadingOptions.Filter.Employee, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Email != null)
                {
                    query = query.Where(w => w.Email.Contains(dataLoadingOptions.Filter.Email, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.PhoneNumber != null)
                {
                    query = query.Where(w => w.PhoneNumber.Contains(dataLoadingOptions.Filter.PhoneNumber, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    query = query.Where(x => x.Department.Company.Name.Contains(dataLoadingOptions.Filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    query = query.Where(x => x.Department.Name.Contains(dataLoadingOptions.Filter.Department, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Position != null)
                {
                    query = query.Where(x => x.Position.Name.Contains(dataLoadingOptions.Filter.Position, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.LastSeenStartDate != null)
                {
                    query = query.Where(w => w.LastSeen >= dataLoadingOptions.Filter.LastSeenStartDate);
                }
                if (dataLoadingOptions.Filter.LastSeenEndDate != null)
                {
                    query = query.Where(x => x.LastSeen <= dataLoadingOptions.Filter.LastSeenEndDate);
                }
                if (dataLoadingOptions.Filter.VaultsCount != null)
                {
                    query = query.Where(x => (x.HardwareVaults.Count + x.SoftwareVaults.Count) == dataLoadingOptions.Filter.VaultsCount);
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => (x.FirstName + " " + x.LastName).Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Email.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.PhoneNumber.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Company.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Position.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    (x.HardwareVaults.Count + x.SoftwareVaults.Count).ToString().Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            return await query.CountAsync();
        }

        public async Task<IList<string>> GetEmployeeVaultIdsAsync(string employeeId)
        {
            var employee = await GetEmployeeByIdAsync(employeeId);
            return employee.HardwareVaults.Select(x => x.Id).ToList();
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            // If the field is NULL then the unique check does not work; therefore, we write empty
            employee.LastName = employee.LastName ?? string.Empty;

            employee.DepartmentId = string.IsNullOrWhiteSpace(employee.DepartmentId) ? null : employee.DepartmentId;
            employee.PositionId = string.IsNullOrWhiteSpace(employee.PositionId) ? null : employee.PositionId;

            var exist = await _employeeRepository.ExistAsync(x => x.FirstName == employee.FirstName && x.LastName == employee.LastName);
            if (exist)
            {
                throw new AlreadyExistException($"{employee.FirstName} {employee.LastName} already exists.");
            }

            return await _employeeRepository.AddAsync(employee);
        }

        public async Task<Employee> ImportEmployeeAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            // If the field is NULL then the unique check does not work, therefore we write empty field
            employee.LastName = employee.LastName ?? string.Empty;

            var employeeByGuid = await _employeeRepository.Query().FirstOrDefaultAsync(x => x.ActiveDirectoryGuid == employee.ActiveDirectoryGuid);
            if (employeeByGuid != null)
            {
                return employeeByGuid;
            }

            var employeeByName = await _employeeRepository.Query().FirstOrDefaultAsync(x => x.FirstName == employee.FirstName && x.LastName == employee.LastName);
            if (employeeByName != null)
            {
                employeeByName.ActiveDirectoryGuid = employee.ActiveDirectoryGuid;
                return await _employeeRepository.UpdateAsync(employeeByName);
            }

            return await _employeeRepository.AddAsync(employee);
        }

        public async Task SyncEmployeeAsync(List<Employee> impotedEmployees)
        {
            if (impotedEmployees == null)
                throw new ArgumentNullException(nameof(impotedEmployees));

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var impotedEmployee in impotedEmployees)
                {
                    // If the field is NULL then the unique check does not work, therefore we write empty field
                    impotedEmployee.LastName ??= string.Empty;

                    var employeeByGuid = await _employeeRepository.Query().FirstOrDefaultAsync(x => x.ActiveDirectoryGuid == impotedEmployee.ActiveDirectoryGuid);
                    if (employeeByGuid != null)
                    {
                        var modified = false;

                        if (employeeByGuid.FirstName != impotedEmployee.FirstName)
                        {
                            employeeByGuid.FirstName = impotedEmployee.FirstName;
                            modified = true;
                        }

                        if (employeeByGuid.LastName != impotedEmployee.LastName)
                        {
                            employeeByGuid.LastName = impotedEmployee.LastName;
                            modified = true;
                        }

                        if (employeeByGuid.Email != impotedEmployee.Email)
                        {
                            employeeByGuid.Email = impotedEmployee.Email;
                            modified = true;
                        }

                        if (employeeByGuid.PhoneNumber != impotedEmployee.PhoneNumber)
                        {
                            employeeByGuid.PhoneNumber = impotedEmployee.PhoneNumber;
                            modified = true;
                        }

                        if (modified)
                        {
                            await _employeeRepository.UpdateAsync(employeeByGuid);
                        }

                        continue;
                    }

                    var employeeByName = await _employeeRepository.Query().FirstOrDefaultAsync(x => x.FirstName == impotedEmployee.FirstName && x.LastName == impotedEmployee.LastName);
                    if (employeeByName != null)
                    {
                        employeeByName.ActiveDirectoryGuid = impotedEmployee.ActiveDirectoryGuid;
                        employeeByName.Email = impotedEmployee.Email;
                        employeeByName.PhoneNumber = impotedEmployee.PhoneNumber;

                        await _employeeRepository.UpdateAsync(employeeByName);
                        continue;
                    }

                    await _employeeRepository.AddAsync(impotedEmployee);
                }

                // Get all current employees which are imported and have hardwawre vauls
                var currentEmployees = await _employeeRepository
                    .Query()
                    .Include(x => x.HardwareVaults)
                    .Where(x => x.ActiveDirectoryGuid != null && x.HardwareVaults.Count > 0)
                    .AsNoTracking()
                    .ToListAsync();

                // Get employees whose access to possession of keys was taken away in the active dirictory
                var impotedEmployeesGuids = impotedEmployees.Select(d => d.ActiveDirectoryGuid).ToList();
                currentEmployees.RemoveAll(x => impotedEmployeesGuids.Contains(x.ActiveDirectoryGuid));

                // Removal of employee hardware vaults from which access was taken away
                foreach (var employee in currentEmployees)
                    foreach (var hardwareVault in employee.HardwareVaults)
                        await RemoveHardwareVaultAsync(hardwareVault.Id, VaultStatusReason.Withdrawal);

                transactionScope.Complete();
            }
        }

        public async Task SyncEmployeeAccessAsync(List<string> membersGuid)
        {
            // Get all current employees which are imported and have hardwawre vauls
            var employees = await _employeeRepository
                .Query()
                .Include(x => x.HardwareVaults)
                .Where(x => x.ActiveDirectoryGuid != null && x.HardwareVaults.Count > 0)
                .AsNoTracking()
                .ToListAsync();

            // Get employees whose access to possession of keys was taken away in the active dirictory
            employees.RemoveAll(x => membersGuid.Contains(x.ActiveDirectoryGuid));

            // Removal of employee hardware vaults from which access was taken away
            foreach (var employee in employees)
                foreach (var hardwareVault in employee.HardwareVaults)
                    await RemoveHardwareVaultAsync(hardwareVault.Id, VaultStatusReason.Withdrawal);
        }

        public async Task EditEmployeeAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            // If the field is NULL then the unique check does not work; therefore, we write empty
            employee.LastName = employee.LastName ?? string.Empty;

            employee.DepartmentId = string.IsNullOrWhiteSpace(employee.DepartmentId) ? null : employee.DepartmentId;
            employee.PositionId = string.IsNullOrWhiteSpace(employee.PositionId) ? null : employee.PositionId;

            var exist = await _employeeRepository.ExistAsync(x => x.FirstName == employee.FirstName && x.LastName == employee.LastName && x.Id != employee.Id);
            if (exist)
            {
                throw new AlreadyExistException($"{employee.FirstName} {employee.LastName} already exists.");
            }

            var properties = new string[]
            {
                nameof(Employee.FirstName),
                nameof(Employee.LastName),
                nameof(Employee.Email),
                nameof(Employee.PhoneNumber),
                nameof(Employee.DepartmentId),
                nameof(Employee.PositionId)
            };

            await _employeeRepository.UpdateOnlyPropAsync(employee, properties);
        }

        public async Task DeleteEmployeeAsync(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var employee = await _employeeRepository.GetByIdAsync(id);

            if (employee == null)
                throw new Exception("Employee not found");

            var hardwareVaults = await _hardwareVaultService
                .VaultQuery()
                .Where(x => x.EmployeeId == id)
                .AnyAsync();

            if (hardwareVaults)
                throw new Exception("First untie the hardware vault before removing.");

            var softwareVaults = await _softwareVaultService
                .SoftwareVaultQuery()
                .Where(x => x.EmployeeId == id)
                .AnyAsync();

            if (softwareVaults)
                throw new Exception("First untie the software vault before removing.");

            await _employeeRepository.DeleteAsync(employee);
        }

        public async Task UpdateLastSeenAsync(string vaultId)
        {
            var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);
            if (vault?.EmployeeId == null)
                return;

            var employee = await _employeeRepository.GetByIdAsync(vault.EmployeeId);
            if (employee == null)
                return;

            employee.LastSeen = DateTime.UtcNow;
            await _employeeRepository.UpdateOnlyPropAsync(employee, new string[] { nameof(Employee.LastSeen) });
        }

        public async Task<bool> CheckEmployeeNameExistAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            // If the field is NULL then the unique check does not work; therefore, we write empty
            employee.LastName = employee.LastName ?? string.Empty;
            return await _employeeRepository.ExistAsync(x => x.FirstName == employee.FirstName && x.LastName == employee.LastName);
        }

        #endregion

        #region Hardware Vault

        public async Task AddHardwareVaultAsync(string employeeId, string vaultId)
        {
            if (employeeId == null)
                throw new ArgumentNullException(nameof(employeeId));

            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            _dataProtectionService.Validate();

            var employee = await GetEmployeeByIdAsync(employeeId);
            if (employee == null)
                throw new Exception("Employee not found");

            if (employee.HardwareVaults.Count > 0)
                throw new Exception("Cannot add more than one hardware vault.");

            var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);
            if (vault == null)
                throw new Exception($"Vault {vault} not found");

            if (vault.Status != VaultStatus.Ready)
                throw new Exception($"Vault {vaultId} in a status that does not allow to reserve.");

            vault.EmployeeId = employeeId;
            vault.Status = VaultStatus.Reserved;
            vault.IsStatusApplied = false;
            vault.MasterPassword = _dataProtectionService.Encrypt(GenerateMasterPassword());

            var accounts = await GetAccountsByEmployeeIdAsync(employeeId);
            var tasks = new List<HardwareVaultTask>();

            // Create a task for accounts that were created without a vault
            foreach (var account in accounts.Where(x => x.Password != null))
            {
                tasks.Add(new HardwareVaultTask
                {
                    Password = account.Password,
                    OtpSecret = account.OtpSecret,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Create,
                    Timestamp = UnixTime.ConvertToUnixTime(DateTime.UtcNow),
                    HardwareVaultId = vault.Id,
                    AccountId = account.Id
                });
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultService.UpdateVaultAsync(vault);
                await _hardwareVaultService.CreateVaultActivationAsync(vaultId);

                if (tasks.Count > 0)
                    await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);

                transactionScope.Complete();
            }
        }

        public async Task RemoveHardwareVaultAsync(string vaultId, VaultStatusReason reason, bool isNeedBackup = false)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            _dataProtectionService.Validate();

            var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);
            if (vault == null)
                throw new Exception($"Vault {vaultId} not found");

            if (vault.Status != VaultStatus.Reserved &&
                vault.Status != VaultStatus.Active &&
                vault.Status != VaultStatus.Locked &&
                vault.Status != VaultStatus.Suspended)
            {
                throw new Exception($"Vault {vaultId} in a status that does not allow to remove.");
            }

            var employeeId = vault.EmployeeId;
            var deleteAccounts = vault.Employee.HardwareVaults.Count() == 1 ? true : false;

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultTaskService.DeleteTasksByVaultIdAsync(vaultId);
                await _workstationService.DeleteProximityByVaultIdAsync(vaultId);

                vault.EmployeeId = null;

                if (vault.Status == VaultStatus.Reserved && !vault.IsStatusApplied)
                {
                    vault.Status = VaultStatus.Ready;
                    vault.MasterPassword = null;
                    await _hardwareVaultService.ChangeVaultActivationStatusAsync(vaultId, HardwareVaultActivationStatus.Canceled);
                }
                else
                {
                    vault.Status = VaultStatus.Deactivated;
                    vault.StatusReason = reason;

                    if (!isNeedBackup && deleteAccounts)
                    {
                        var employee = await GetEmployeeByIdAsync(employeeId);
                        employee.PrimaryAccountId = null;
                        await _employeeRepository.UpdateAsync(employee);
                        await _accountService.DeleteAccountsByEmployeeIdAsync(employeeId);
                    }
                }

                await _hardwareVaultService.UpdateVaultAsync(vault);

                transactionScope.Complete();
            }
        }

        #endregion

        #region Account

        public async Task ReloadAccountAsync(string accountId)
        {
            await _accountService.ReloadAccountAsync(accountId);
        }

        public async Task<Account> GetAccountByIdAsync(string accountId)
        {
            return await _accountService
                .Query()
                .Include(x => x.Employee.HardwareVaults)
                .Include(x => x.SharedAccount)
                .FirstOrDefaultAsync(x => x.Id == accountId);
        }

        public async Task<List<Account>> GetAccountsAsync(DataLoadingOptions<AccountFilter> dataLoadingOptions)
        {
            var query = _accountService
                .Query()
                .Include(x => x.Employee.HardwareVaults)
                .Include(x => x.SharedAccount)
                .Where(x => x.EmployeeId == dataLoadingOptions.EntityId && x.Deleted == false)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x =>
                                    x.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Urls.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Apps.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Login.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(Account.Name):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
                    break;
                case nameof(Account.Urls):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Urls) : query.OrderByDescending(x => x.Urls);
                    break;
                case nameof(Account.Apps):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Apps) : query.OrderByDescending(x => x.Apps);
                    break;
                case nameof(Account.Login):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Login) : query.OrderByDescending(x => x.Login);
                    break;
                case nameof(Account.Type):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Type) : query.OrderByDescending(x => x.Type);
                    break;
                case nameof(Account.CreatedAt):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt);
                    break;
                case nameof(Account.UpdatedAt):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.UpdatedAt) : query.OrderByDescending(x => x.UpdatedAt);
                    break;
                case nameof(Account.PasswordUpdatedAt):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.PasswordUpdatedAt) : query.OrderByDescending(x => x.PasswordUpdatedAt);
                    break;
                case nameof(Account.OtpUpdatedAt):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.OtpUpdatedAt) : query.OrderByDescending(x => x.OtpUpdatedAt);
                    break;
            }

            return await query.Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetAccountsCountAsync(DataLoadingOptions<AccountFilter> dataLoadingOptions)
        {
            var query = _accountService
                .Query()
                .Include(x => x.Employee.HardwareVaults)
                .Include(x => x.SharedAccount)
                .Where(x => x.EmployeeId == dataLoadingOptions.EntityId && x.Deleted == false)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x =>
                                    x.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Urls.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Apps.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Login.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            return await query.CountAsync();
        }

        public async Task<List<Account>> GetAccountsByEmployeeIdAsync(string employeeId)
        {
            return await _accountService
                .Query()
                .Include(x => x.Employee.HardwareVaults)
                .Include(x => x.SharedAccount)
                .Where(x => x.EmployeeId == employeeId && x.Deleted == false)
                .ToListAsync();
        }

        public async Task<Account> CreatePersonalAccountAsync(PersonalAccount personalAccount, bool isWorkstationAccount = false)
        {
            if (personalAccount == null)
                throw new ArgumentNullException(nameof(personalAccount));

            _dataProtectionService.Validate();

            var exist = await _accountService.ExistAsync(x => x.EmployeeId == personalAccount.EmployeeId &&
                                                         x.Name == personalAccount.Name &&
                                                         x.Login == personalAccount.Login &&
                                                         x.Deleted == false);
            if (exist)
                throw new AlreadyExistException("An account with the same name and login exist.");

            var account = new Account()
            {
                Id = Guid.NewGuid().ToString(),
                Name = personalAccount.Name,
                Urls = Validation.VerifyUrls(personalAccount.Urls),
                Apps = personalAccount.Apps,
                Login = personalAccount.Login,
                Type = AccountType.Personal,
                Kind = isWorkstationAccount ? AccountKind.Workstation : AccountKind.WebApp,
                CreatedAt = DateTime.UtcNow,
                PasswordUpdatedAt = DateTime.UtcNow,
                OtpUpdatedAt = Validation.VerifyOtpSecret(personalAccount.OtpSecret) != null ? new DateTime?(DateTime.UtcNow) : null,
                Password = _dataProtectionService.Encrypt(personalAccount.Password),
                OtpSecret = _dataProtectionService.Encrypt(personalAccount.OtpSecret),
                UpdateInActiveDirectory = personalAccount.UpdateInActiveDirectory,
                EmployeeId = personalAccount.EmployeeId,
                StorageId = new StorageId().Data
            };

            Employee employee = await GetEmployeeByIdAsync(personalAccount.EmployeeId);
            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();

            foreach (var vault in employee.HardwareVaults)
            {
                tasks.Add(new HardwareVaultTask
                {
                    Password = _dataProtectionService.Encrypt(personalAccount.Password),
                    OtpSecret = _dataProtectionService.Encrypt(personalAccount.OtpSecret),
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Create,
                    Timestamp = UnixTime.ConvertToUnixTime(DateTime.UtcNow),
                    HardwareVaultId = vault.Id,
                    AccountId = account.Id
                });
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _accountService.AddAsync(account);
                await SetAsWorkstationAccountIfEmptyAsync(account.EmployeeId, account.Id);

                if (tasks.Count > 0)
                {
                    await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                    employee.HardwareVaults.ForEach(x => x.NeedSync = true);
                    await _hardwareVaultService.UpdateRangeVaultsAsync(employee.HardwareVaults);
                }

                transactionScope.Complete();
            }

            return account;
        }

        public async Task<Account> CreateWorkstationAccountAsync(WorkstationAccount workstationAccount)
        {
            if (workstationAccount == null)
                throw new ArgumentNullException(nameof(workstationAccount));

            switch (workstationAccount.Type)
            {
                case WorkstationAccountType.Local:
                    await ValidateAccountNameAndLoginAsync(workstationAccount.EmployeeId, workstationAccount.Name, $".\\{workstationAccount.UserName}");
                    workstationAccount.UserName = $".\\{workstationAccount.UserName}";
                    break;
                case WorkstationAccountType.AzureAD:
                    await ValidateAccountNameAndLoginAsync(workstationAccount.EmployeeId, workstationAccount.Name, $"AzureAD\\{workstationAccount.UserName}");
                    workstationAccount.UserName = $"AzureAD\\{workstationAccount.UserName}";
                    break;
                case WorkstationAccountType.Microsoft:
                    await ValidateAccountNameAndLoginAsync(workstationAccount.EmployeeId, workstationAccount.Name, $"@\\{workstationAccount.UserName}");
                    workstationAccount.UserName = $"@\\{workstationAccount.UserName}";
                    break;
            }

            var personalAccount = new PersonalAccount()
            {
                Name = workstationAccount.Name,
                Login = workstationAccount.UserName,
                Password = workstationAccount.Password,
                EmployeeId = workstationAccount.EmployeeId
            };

            return await CreatePersonalAccountAsync(personalAccount, isWorkstationAccount: true);
        }

        public async Task<Account> CreateWorkstationAccountAsync(WorkstationDomain workstationAccount)
        {
            if (workstationAccount == null)
                throw new ArgumentNullException(nameof(workstationAccount));

            await ValidateAccountNameAndLoginAsync(workstationAccount.EmployeeId, workstationAccount.Name, $"{workstationAccount.Domain}\\{workstationAccount.UserName}");

            var personalAccount = new PersonalAccount()
            {
                Name = workstationAccount.Name,
                Login = $"{workstationAccount.Domain}\\{workstationAccount.UserName}",
                Password = workstationAccount.Password,
                EmployeeId = workstationAccount.EmployeeId,
                UpdateInActiveDirectory = workstationAccount.UpdateInActiveDirectory
            };

            return await CreatePersonalAccountAsync(personalAccount, isWorkstationAccount: true);
        }

        private async Task ValidateAccountNameAndLoginAsync(string employeeId, string name, string login)
        {
            var exist = await _accountService.ExistAsync(x => x.EmployeeId == employeeId &&
                                            x.Name == name && x.Login == login &&
                                            x.Deleted == false);
            if (exist)
                throw new AlreadyExistException("An account with the same name and login exist.");
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

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                employee.PrimaryAccountId = accountId;
                await _employeeRepository.UpdateOnlyPropAsync(employee, new string[] { nameof(Employee.PrimaryAccountId) });

                foreach (var vault in employee.HardwareVaults)
                {
                    await _hardwareVaultTaskService.AddPrimaryAsync(vault.Id, accountId);

                    vault.NeedSync = true;
                    await _hardwareVaultService.UpdateVaultAsync(vault);
                }

                transactionScope.Complete();
            }
        }

        private async Task SetAsWorkstationAccountIfEmptyAsync(string employeeId, string accountId)
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

            var exist = await _accountService.ExistAsync(x => x.Name == account.Name &&
                                                         x.Login == account.Login &&
                                                         x.Deleted == false &&
                                                         x.EmployeeId == account.EmployeeId &&
                                                         x.Id != account.Id);
            if (exist)
                throw new AlreadyExistException("An account with the same name and login exist.");

            var employee = await GetEmployeeByIdAsync(account.EmployeeId);

            account.Urls = Validation.VerifyUrls(account.Urls);
            account.UpdatedAt = DateTime.UtcNow;

            // Create tasks if there are vaults
            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();
            foreach (var vault in employee.HardwareVaults)
            {
                tasks.Add(new HardwareVaultTask
                {
                    Operation = TaskOperation.Update,
                    CreatedAt = DateTime.UtcNow,
                    Timestamp = UnixTime.ConvertToUnixTime(DateTime.UtcNow),
                    HardwareVaultId = vault.Id,
                    AccountId = account.Id
                });
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _accountService.UpdateOnlyPropAsync(account, new string[] { nameof(Account.Name), nameof(Account.Login), nameof(Account.Urls), nameof(Account.Apps), nameof(Account.UpdatedAt) });

                if (tasks.Count > 0)
                {
                    await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                    employee.HardwareVaults.ForEach(x => x.NeedSync = true);
                    await _hardwareVaultService.UpdateRangeVaultsAsync(employee.HardwareVaults);
                }

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

            account.UpdatedAt = DateTime.UtcNow;
            account.PasswordUpdatedAt = DateTime.UtcNow;

            // Update password field if there are no vaults
            if (employee.HardwareVaults.Count == 0)
                account.Password = _dataProtectionService.Encrypt(accountPassword.Password);

            // Create tasks if there are vaults
            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();
            foreach (var device in employee.HardwareVaults)
            {
                tasks.Add(new HardwareVaultTask
                {
                    Password = _dataProtectionService.Encrypt(accountPassword.Password),
                    Operation = TaskOperation.Update,
                    CreatedAt = DateTime.UtcNow,
                    Timestamp = UnixTime.ConvertToUnixTime(DateTime.UtcNow),
                    HardwareVaultId = device.Id,
                    AccountId = account.Id
                });
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _accountService.UpdateOnlyPropAsync(account, new string[] { nameof(Account.UpdatedAt), nameof(Account.PasswordUpdatedAt), nameof(Account.Password) });

                if (tasks.Count > 0)
                {
                    await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                    employee.HardwareVaults.ForEach(x => x.NeedSync = true);
                    await _hardwareVaultService.UpdateRangeVaultsAsync(employee.HardwareVaults);
                }

                transactionScope.Complete();
            }
        }

        public async Task EditPersonalAccountOtpAsync(Account account, AccountOtp accountOtp)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            if (accountOtp == null)
                throw new ArgumentNullException(nameof(accountOtp));

            _dataProtectionService.Validate();

            var employee = await GetEmployeeByIdAsync(account.EmployeeId);

            account.UpdatedAt = DateTime.UtcNow;
            account.OtpUpdatedAt = Validation.VerifyOtpSecret(accountOtp.OtpSecret) == null ? null : (DateTime?)DateTime.UtcNow;

            // Update otp field if there are no vaults
            if (employee.HardwareVaults.Count == 0)
                account.OtpSecret = _dataProtectionService.Encrypt(accountOtp.OtpSecret);

            // Create tasks if there are vaults
            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();
            foreach (var device in employee.HardwareVaults)
            {
                tasks.Add(new HardwareVaultTask
                {
                    OtpSecret = _dataProtectionService.Encrypt(accountOtp.OtpSecret ?? string.Empty),
                    Operation = TaskOperation.Update,
                    CreatedAt = DateTime.UtcNow,
                    Timestamp = UnixTime.ConvertToUnixTime(DateTime.UtcNow),
                    HardwareVaultId = device.Id,
                    AccountId = account.Id
                });
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _accountService.UpdateOnlyPropAsync(account, new string[] { nameof(Account.UpdatedAt), nameof(Account.OtpUpdatedAt), nameof(Account.OtpSecret) });

                if (tasks.Count > 0)
                {
                    await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                    employee.HardwareVaults.ForEach(x => x.NeedSync = true);
                    await _hardwareVaultService.UpdateRangeVaultsAsync(employee.HardwareVaults);
                }

                transactionScope.Complete();
            }
        }

        public Task UnchangedPersonalAccountAsync(Account account)
        {
            return _accountService.UnchangedAsync(account);
        }

        public async Task<Account> AddSharedAccountAsync(string employeeId, string sharedAccountId)
        {
            if (employeeId == null)
                throw new ArgumentNullException(nameof(employeeId));

            if (sharedAccountId == null)
                throw new ArgumentNullException(nameof(sharedAccountId));

            _dataProtectionService.Validate();

            var sharedAccount = await _sharedAccountService.GetSharedAccountByIdAsync(sharedAccountId);
            if (sharedAccount == null)
                throw new Exception("Shared Account not found");

            var exist = await _accountService.ExistAsync(x => x.EmployeeId == employeeId &&
                                                         x.Name == sharedAccount.Name &&
                                                         x.Login == sharedAccount.Login &&
                                                         x.Deleted == false);
            if (exist)
                throw new Exception("An account with the same name and login exists");

            var account = new Account
            {
                Id = Guid.NewGuid().ToString(),
                Name = sharedAccount.Name,
                Urls = sharedAccount.Urls,
                Apps = sharedAccount.Apps,
                Login = sharedAccount.Login,
                Type = AccountType.Shared,
                Kind = sharedAccount.Kind,
                CreatedAt = DateTime.UtcNow,
                PasswordUpdatedAt = DateTime.UtcNow,
                OtpUpdatedAt = sharedAccount.OtpSecret != null ? new DateTime?(DateTime.UtcNow) : null,
                EmployeeId = employeeId,
                SharedAccountId = sharedAccountId,
                Password = sharedAccount.Password,
                OtpSecret = sharedAccount.OtpSecret,
                StorageId = new StorageId().Data
            };

            var employee = await GetEmployeeByIdAsync(employeeId);
            var tasks = new List<HardwareVaultTask>();

            foreach (var device in employee.HardwareVaults)
            {
                tasks.Add(new HardwareVaultTask
                {
                    Password = sharedAccount.Password,
                    OtpSecret = sharedAccount.OtpSecret,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Create,
                    Timestamp = UnixTime.ConvertToUnixTime(DateTime.UtcNow),
                    HardwareVaultId = device.Id,
                    AccountId = account.Id
                });
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _accountService.AddAsync(account);
                await SetAsWorkstationAccountIfEmptyAsync(account.EmployeeId, account.Id);

                if (tasks.Count > 0)
                {
                    await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                    employee.HardwareVaults.ForEach(x => x.NeedSync = true);
                    await _hardwareVaultService.UpdateRangeVaultsAsync(employee.HardwareVaults);
                }

                transactionScope.Complete();
            }

            return account;
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

            var isPrimary = employee.PrimaryAccountId == accountId;
            if (isPrimary)
                employee.PrimaryAccountId = null;

            account.Deleted = true;
            account.UpdatedAt = DateTime.UtcNow;
            account.Password = null;
            account.OtpSecret = null;

            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();

            foreach (var vault in employee.HardwareVaults)
            {
                tasks.Add(new HardwareVaultTask
                {
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Delete,
                    Timestamp = UnixTime.ConvertToUnixTime(DateTime.UtcNow),
                    HardwareVaultId = vault.Id,
                    AccountId = account.Id
                });
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                if (isPrimary)
                    await _employeeRepository.UpdateOnlyPropAsync(employee, new string[] { nameof(Employee.PrimaryAccountId) });

                await _accountService.UpdateOnlyPropAsync(account, new string[] { nameof(Account.Deleted), nameof(Account.UpdatedAt), nameof(Account.Password), nameof(Account.OtpSecret) });
                await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);

                employee.HardwareVaults.ForEach(x => x.NeedSync = true);
                await _hardwareVaultService.UpdateRangeVaultsAsync(employee.HardwareVaults);

                transactionScope.Complete();
            }

            return account;
        }

        private string GenerateMasterPassword()
        {
            var buf = AesCryptoHelper.CreateRandomBuf(32);
            for (int i = 0; i < 32; i++)
            {
                if (buf[i] == 0)
                    buf[i] = 0xff;
            }
            var pass = ConvertUtils.ByteArrayToHexString(buf);
            return pass;
        }

        #endregion

        public void Dispose()
        {
            _employeeRepository.Dispose();
            _hardwareVaultService.Dispose();
            _hardwareVaultTaskService.Dispose();
            _softwareVaultService.Dispose();
            _accountService.Dispose();
            _sharedAccountService.Dispose();
            _workstationService.Dispose();
        }
    }
}