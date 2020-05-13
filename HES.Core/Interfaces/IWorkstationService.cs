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
        IQueryable<Workstation> WorkstationQuery();
        Task<Workstation> GetWorkstationByIdAsync(string id);
        Task<List<Workstation>> GetWorkstationsAsync();
        Task<List<Workstation>> GetFilteredWorkstationsAsync(WorkstationFilter workstationFilter);
        Task<bool> ExistAsync(Expression<Func<Workstation, bool>> predicate);
        Task AddWorkstationAsync(WorkstationInfo workstationInfo);
        Task UpdateWorkstationInfoAsync(WorkstationInfo workstationInfo);
        Task EditWorkstationAsync(Workstation workstation);
        Task ApproveWorkstationAsync(Workstation workstation);
        Task UnapproveWorkstationAsync(string id);
        Task<bool> GetRfidStateAsync(string workstationId);
        Task UpdateRfidStateAsync(string workstationId);
        IQueryable<WorkstationProximityVault> ProximityDeviceQuery();
        Task<List<WorkstationProximityVault>> GetProximityDevicesAsync(string workstationId);
        Task<WorkstationProximityVault> GetProximityDeviceByIdAsync(string id);
        Task<IList<WorkstationProximityVault>> AddProximityDevicesAsync(string workstationId, string[] devicesIds);
        Task AddMultipleProximityDevicesAsync(string[] workstationsIds, string[] devicesIds);
        Task EditProximityDeviceAsync(WorkstationProximityVault proximityDevice);
        Task DeleteProximityVaultAsync(string proximityDeviceId);
        Task DeleteRangeProximityVaultsAsync(List<WorkstationProximityVault> proximityDevices);
        Task DeleteProximityByVaultIdAsync(string deviceId);
        Task UpdateProximitySettingsAsync(string workstationId);
    }
}