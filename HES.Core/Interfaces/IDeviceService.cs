using HES.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceService
    {
        IQueryable<Device> Query();
        Task<int> GetCountAsync();
        Task<Device> GetByIdAsync(dynamic id);
        Task<(IList<Device> devicesExists, IList<Device> devicesImported, string message)> ImportDevices(string key, byte[] fileContent);
        Task EditRfidAsync(Device device);
        Task UpdateOnlyPropAsync(Device device, string[] properties);
        Task UpdateDeviceInfoAsync(string deviceId, int battery, string firmware, bool locked);
        Task<string[]> GetDevicesByProfileAsync(string profileId);
        Task SetProfileAsync(string[] devicesId, string profileId);
        Task<string[]> UpdateProfileAsync(string profileId);
        Task UnlockPinAsync(string deviceId);
        Task<bool> ExistAsync(Expression<Func<Device, bool>> predicate);
        Task RemoveEmployeeAsync(string deviceId);
        Task<int> GetFreeDevicesCount();
    }
}