using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Models;
using HES.Core.Models.Employees;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Account;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IEmployeeService
    {
        Task<Employee> CreateEmployeeAsync(Employee employee);
        IQueryable<Employee> EmployeeQuery();
        Task<int> GetEmployeesCountAsync(DataLoadingOptions<EmployeeFilter> dataLoadingOptions);
        Task<List<Employee>> GetEmployeesAsync(DataLoadingOptions<EmployeeFilter> dataLoadingOptions);
        Task<Employee> GetEmployeeByIdAsync(string id);
        Task<bool> CheckEmployeeNameExistAsync(Employee employee);
        Task EditEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(string id);
        Task<Account> CreatePersonalAccountAsync(PersonalAccount personalAccount, bool isWorkstationAccount = false);
        Task<Account> CreateWorkstationAccountAsync(WorkstationAccount workstationAccount);
        Task<Account> CreateWorkstationAccountAsync(WorkstationDomain workstationAccount);
        Task<int> GetAccountsCountAsync(string searchText, string employeeId);
        Task<List<Account>> GetAccountsAsync(int skip, int take, string sortColumn, ListSortDirection sortDirection, string searchText, string employeeId);
        Task<List<Account>> GetAccountsByEmployeeIdAsync(string employeeId);
        Task SetAsWorkstationAccountAsync(string employeeId, string accountId);
        Task<Account> GetAccountByIdAsync(string accountId);
        Task UnchangedPersonalAccountAsync(Account account);
        Task EditPersonalAccountAsync(Account account);
        Task EditPersonalAccountPwdAsync(Account account, AccountPassword accountPassword);
        Task EditPersonalAccountOtpAsync(Account account, AccountOtp accountOtp);
        Task<Account> DeleteAccountAsync(string accountId);


        //Not covered by tests
        Task<IList<string>> GetEmployeeVaultIdsAsync(string employeeId);
        Task UnchangedEmployeeAsync(Employee employee);
        Task<Employee> ImportEmployeeAsync(Employee employee);
        Task UpdateLastSeenAsync(string vaultId);
        Task AddHardwareVaultAsync(string employeeId, string vaultId);
        Task RemoveHardwareVaultAsync(string vaultId, VaultStatusReason reason, bool isNeedBackup = false);
        Task<Account> AddSharedAccountAsync(string employeeId, string sharedAccountId);
    }
}