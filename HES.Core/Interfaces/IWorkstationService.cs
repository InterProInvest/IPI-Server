using HES.Core.Entities;
using HES.Core.Models;
using Hideez.SDK.Communication.Workstation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationService
    {
        IQueryable<Workstation> QueryOfWorkstation();
        Task<Workstation> GetWorkstationByIdAsync(string id);
        Task<List<Workstation>> GetWorkstationsAsync();
        Task<List<Workstation>> GetFilteredWorkstationsAsync(WorkstationFilter workstationFilter);
        Task<int> GetOnlineCountAsync();
        Task<bool> ExistAsync(Expression<Func<Workstation, bool>> predicate);
        Task AddWorkstationAsync(WorkstationInfo workstationInfo);
        Task UpdateWorkstationInfoAsync(WorkstationInfo workstationInfo);
        Task EditWorkstationAsync(Workstation workstation);
        Task ApproveWorkstationAsync(Workstation workstation);
        Task UnapproveWorkstationAsync(string id);
        Task<bool> GetRfidStateAsync(string workstationId);
        Task UpdateRfidStateAsync(string workstationId);
        IQueryable<ProximityDevice> QueryOfProximityDevice();
        Task<List<ProximityDevice>> GetProximityDevicesAsync(string workstationId);
        Task AddProximityDevicesAsync(string workstationId, string[] devicesIds);
        Task AddMultipleProximityDevicesAsync(string[] workstationsIds, string[] devicesIds);
        Task EditProximityDeviceAsync(ProximityDevice proximityDevice);
        Task DeleteProximityDeviceAsync(string proximityDeviceId);
        Task DeleteRangeProximityDevicesAsync(List<ProximityDevice> proximityDevices);
        Task RemoveAllProximityByDeviceIdAsync(string deviceId);
        Task UpdateProximitySettingsAsync(string workstationId);
    }
}