using HES.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceAccountService
    {
        IQueryable<DeviceAccount> Query();
        Task<DeviceAccount> GetByIdAsync(string accountId);
        Task<DeviceAccount> AddAsync(DeviceAccount deviceAccount);
        Task<IList<DeviceAccount>> AddRangeAsync(IList<DeviceAccount> deviceAccounts);
        Task UpdateOnlyPropAsync(DeviceAccount deviceAccount, string[] properties);
        Task UpdateOnlyPropAsync(IList<DeviceAccount> deviceAccounts, string[] properties);
        Task DeleteAsync(DeviceAccount deviceAccount);
        Task DeleteRangeAsync(IList<DeviceAccount> deviceAccounts);
        Task RemoveAllByEmployeeIdAsync(string employeeId);
        Task RemoveAllByDeviceIdAsync(string deviceId);
    }
}