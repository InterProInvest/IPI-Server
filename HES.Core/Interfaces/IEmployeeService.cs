using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Models;
using System;
using System.Collections.Generic;
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
        Task EditEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(string id);
        Task<bool> ExistAsync(Expression<Func<Employee, bool>> predicate);
        Task UpdateLastSeenAsync(string deviceId);
        Task AddDeviceAsync(string employeeId, string[] devices);
        Task RemoveDeviceAsync(string employeeId, string deviceId, VaultStatusReason reason);
        //Task CreateSamlIdpAccountAsync(string email, string password, string hesUrl, string deviceId);
        //Task UpdatePasswordSamlIdpAccountAsync(string email, string password);
        //Task UpdateOtpSamlIdpAccountAsync(string email, string otp);
        //Task<IList<string>> UpdateUrlSamlIdpAccountAsync(string hesUrl);
        //Task DeleteSamlIdpAccountAsync(string employeeId);
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
        //Task HandlingMasterPasswordErrorAsync(string deviceId);
    }
}
