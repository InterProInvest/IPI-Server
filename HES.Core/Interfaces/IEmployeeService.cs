using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IEmployeeService
    {
        IQueryable<Employee> EmployeeQuery();
        Task<Employee> GetEmployeeByIdAsync(string id);
        Task<Employee> GetEmployeeByFullNameAsync(Employee employee);
        Task<List<Employee>> GetEmployeesAsync();
        Task<IList<string>> GetEmployeeDevicesAsync(string employeeId);
        Task<List<Employee>> GetFilteredEmployeesAsync(EmployeeFilter employeeFilter);
        Task<Employee> CreateEmployeeAsync(Employee employee);
        Task<Employee> ImportEmployeeAsync(Employee employee);
        Task EditEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(string id);
        Task<bool> ExistAsync(Expression<Func<Employee, bool>> predicate);
        Task UpdateLastSeenAsync(string vaultId);
        Task AddHardwareVaultAsync(string employeeId, string[] vaultsIds);
        Task RemoveHardwareVaultAsync(string employeeId, string vaultId, VaultStatusReason reason);
        Task<List<Account>> GetAccountsAsync(int skip, int take, string sortColumn, ListSortDirection sortDirection, string searchText, string employeeId);
        Task<int> GetAccountsCountAsync(string searchText, string employeeId);
        Task<List<Account>> GetAccountsByEmployeeIdAsync(string employeeId);
        Task<Account> GetAccountByIdAsync(string accountId);
        Task<Account> CreatePersonalAccountAsync(Account account, AccountPassword accountPassword);
        Task<Account> CreateWorkstationAccountAsync(WorkstationAccount workstationAccount, string employeeId);
        Task SetAsWorkstationAccountAsync(string employeeId, string accountId);
        Task EditPersonalAccountAsync(Account account);
        Task EditPersonalAccountPwdAsync(Account account, AccountPassword accountPassword);
        Task EditPersonalAccountOtpAsync(Account account, AccountPassword accountPassword);
        Task<Account> AddSharedAccountAsync(string employeeId, string sharedAccountId);
        Task<Account> DeleteAccountAsync(string accountId);
    }
}
