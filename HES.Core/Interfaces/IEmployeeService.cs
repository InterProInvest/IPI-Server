using HES.Core.Entities;
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
        Task<List<Employee>> GetAllEmployeesAsync();
        Task<List<Employee>> GetFilteredEmployeesAsync(EmployeeFilter employeeFilter);
        Task<Employee> CreateEmployeeAsync(Employee employee);
        Task EditEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(string id);
        Task<bool> ExistAsync(Expression<Func<Employee, bool>> predicate);
        Task AddDeviceAsync(string employeeId, string[] selectedDevices);
        Task RemoveDeviceAsync(string employeeId, string deviceId);
        Task CreateSamlIdpAccountAsync(string email, string password, string hesUrl, string deviceId);
        Task UpdatePasswordSamlIdpAccountAsync(string email, string password);
        Task UpdateOtpSamlIdpAccountAsync(string email, string otp);
        Task<IList<string>> UpdateUrlSamlIdpAccountAsync(string hesUrl);
        Task DeleteSamlIdpAccountAsync(string employeeId);
        Task<List<DeviceAccount>> GetDeviceAccountsByEmployeeIdAsync(string employeeId);
        Task<DeviceAccount> GetDeviceAccountByIdAsync(string deviceAccountId);
        Task SetAsWorkstationAccountAsync(string deviceId, string deviceAccountId);
        Task<IList<DeviceAccount>> CreateWorkstationAccountAsync(WorkstationAccount workstationAccount, string employeeId, string deviceId);
        Task<IList<DeviceAccount>> CreatePersonalAccountAsync(DeviceAccount deviceAccount, AccountPassword accountPassword, string[] selectedDevices);
        Task EditPersonalAccountAsync(DeviceAccount deviceAccount);
        Task EditPersonalAccountPwdAsync(DeviceAccount deviceAccount, AccountPassword accountPassword);
        Task EditPersonalAccountOtpAsync(DeviceAccount deviceAccount, AccountPassword accountPassword);
        Task<IList<DeviceAccount>> AddSharedAccountAsync(string employeeId, string sharedAccountId, string[] selectedDevices);
        Task<string> DeleteAccountAsync(string accountId);
        Task<DeviceAccount> GetLastChangedAccountAsync(string deviceId);
        Task UndoChangesAsync(string deviceId);
        Task HandlingMasterPasswordErrorAsync(string deviceId);
    }
}