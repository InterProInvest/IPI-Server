using HES.Core.Entities;
using HES.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceService
    {
        IQueryable<Device> QueryOfDevice();
        Task<Device> GetDeviceByIdAsync(dynamic id);
        Task<Device> GetByIdWithIncludeAsync(string id);
        Task<List<Device>> GetDevicesAsync();
        Task<List<Device>> GetFilteredDevicesAsync(DeviceFilter deviceFilter);
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
    }
}