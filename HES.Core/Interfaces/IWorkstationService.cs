using HES.Core.Entities;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Workstations;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Workstation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationService
    {
        IQueryable<Workstation> WorkstationQuery();
        Task<Workstation> GetWorkstationByIdAsync(string id);
        Task<List<Workstation>> GetWorkstationsAsync(DataLoadingOptions<WorkstationFilter> dataLoadingOptions);
        Task<int> GetWorkstationsCountAsync(DataLoadingOptions<WorkstationFilter> dataLoadingOptions);
        Task<bool> ExistAsync(Expression<Func<Workstation, bool>> predicate);
        Task AddWorkstationAsync(WorkstationInfo workstationInfo);
        Task UpdateWorkstationInfoAsync(WorkstationInfo workstationInfo);
        Task EditWorkstationAsync(Workstation workstation);
        Task ApproveWorkstationAsync(Workstation workstation);
        Task UnapproveWorkstationAsync(string workstationId);
        Task DeleteWorkstationAsync(string workstationId);
        Task<bool> GetRfidStateAsync(string workstationId);
        Task<bool> CheckIsApprovedAsync(string workstationId);
        Task ReloadWorkstationAsync(string workstationId);
        Task UnchangedWorkstationAsync(Workstation workstation);
        IQueryable<WorkstationProximityVault> ProximityVaultQuery();
        Task<WorkstationProximityVault> GetProximityVaultByIdAsync(string id);
        Task<List<WorkstationProximityVault>> GetProximityVaultsAsync(int skip, int take, string sortColumn, ListSortDirection sortDirection, string searchText, string workstationId);
        Task<int> GetProximityVaultsCountAsync(string searchText, string workstationId);
        Task<WorkstationProximityVault> AddProximityVaultAsync(string workstationId, string vaultId);
        Task DeleteProximityVaultAsync(string proximityVaultId);
        Task DeleteProximityByVaultIdAsync(string vaultId);
        Task DetachdProximityVaultsAsync(List<WorkstationProximityVault> workstationProximityVaults);
        Task<IReadOnlyList<DeviceProximitySettingsDto>> GetProximitySettingsAsync(string workstationId);
    }
}