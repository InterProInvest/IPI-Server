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
        Task<bool> CheckIsApprovedAsync(string workstationId);
        IQueryable<WorkstationProximityVault> ProximityVaultQuery();
        Task<List<WorkstationProximityVault>> GetProximityVaultsByWorkstationIdAsync(string workstationId);
        Task<WorkstationProximityVault> GetProximityVaultByIdAsync(string id);
        Task<IList<WorkstationProximityVault>> AddProximityVaultsAsync(string workstationId, string[] vaultsIds);
        Task AddMultipleProximityVaultsAsync(string[] workstationsIds, string[] vaultsIds);
        Task EditProximityVaultAsync(WorkstationProximityVault proximityVault);
        Task DeleteProximityVaultAsync(string proximityVaultId);
        Task DeleteRangeProximityVaultsAsync(List<WorkstationProximityVault> proximityVaults);
        Task DeleteProximityByVaultIdAsync(string vaultId);
        Task UpdateProximitySettingsAsync(string workstationId);
    }
}