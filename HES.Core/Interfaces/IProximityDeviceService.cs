using HES.Core.Entities;
using Hideez.SDK.Communication.HES.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IProximityDeviceService
    {
        IQueryable<ProximityDevice> Query();
        Task AddProximityDeviceAsync(string workstationId, string[] devicesId);
        Task AddMultipleProximityDevicesAsync(string[] workstationsId, string[] devicesId);
        Task EditProximityDeviceAsync(ProximityDevice proximityDevice);
        Task DeleteProximityDeviceAsync(string proximityDeviceId);
        Task DeleteRangeProximityDevicesAsync(List<ProximityDevice> proximityDevices);
        Task<IReadOnlyList<DeviceProximitySettingsDto>> GetProximitySettingsAsync(string workstationId);
        Task UpdateProximitySettingsAsync(string workstationId);
        Task RemoveAllProximityAsync(string deviceId);
    }
}