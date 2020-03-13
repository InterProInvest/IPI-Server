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
    public interface IDeviceService
    {
        IQueryable<Device> DeviceQuery();
        Task<Device> GetDeviceByIdAsync(string id);
        Task<List<Device>> GetDevicesByEmployeeIdAsync(string id);
        Task<List<Device>> GetDevicesAsync();
        Task<List<Device>> GetFilteredDevicesAsync(DeviceFilter deviceFilter);
        Task<Device> AddDeviceAsync(Device device);
        Task<(IList<Device> devicesExists, IList<Device> devicesImported, string message)> ImportDevicesAsync(string key, byte[] fileContent);
        Task ImportDevicesAsync();
        Task EditRfidAsync(Device device);
        Task UpdateOnlyPropAsync(Device device, string[] properties);
        Task UpdateDeviceInfoAsync(string deviceId, int battery, string firmware, bool locked);
        Task UpdateNeedSyncAsync(Device device, bool needSync);
        Task UpdateNeedSyncAsync(IList<Device> devices, bool needSync);
        Task UnlockPinAsync(string deviceId);
        Task<bool> ExistAsync(Expression<Func<Device, bool>> predicate);
        Task RemoveEmployeeAsync(string deviceId);
        Task RestoreDefaultsAsync(string deviceId);
        Task SetDeviceStateAsync(string deviceId, DeviceState deviceState);
        IQueryable<DeviceAccessProfile> AccessProfileQuery();
        Task<List<DeviceAccessProfile>> GetAccessProfilesAsync();
        Task<DeviceAccessProfile> GetAccessProfileByIdAsync(string id);
        Task<DeviceAccessProfile> CreateProfileAsync(DeviceAccessProfile deviceAccessProfile);
        Task EditProfileAsync(DeviceAccessProfile deviceAccessProfile);
        Task DeleteProfileAsync(string id);
        Task SetProfileAsync(string[] devicesId, string profileId);
        Task<string[]> UpdateProfileAsync(string profileId);
    }
}