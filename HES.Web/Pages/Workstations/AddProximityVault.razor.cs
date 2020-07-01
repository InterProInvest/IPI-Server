using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web;
using HES.Core.Models.Web.HardwareVaults;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public partial class AddProximityVault : ComponentBase
    {
        [Inject] IWorkstationService WorkstationService { get; set; }
        [Inject] IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] ILogger<AddProximityVault> Logger { get; set; }
        [Inject] IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }
        [Parameter] public string WorkstationId { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public List<HardwareVault> HardwareVaults { get; set; }
        public HardwareVault SelectedHardwareVault { get; set; }
        public string WarningMessage { get; set; }
        public bool AlreadyAdded { get; set; }

        protected override async Task OnInitializedAsync()
        {
            SearchText = string.Empty;
            await LoadDataAsync();
        }

        public int TotalRecords { get; set; }
        public string SearchText { get; set; }

        private async Task LoadDataAsync()
        {
            var filter = new HardwareVaultFilter() { Status = VaultStatus.Ready };
            TotalRecords = await HardwareVaultService.GetVaultsCountAsync(new DataLoadingOptions<HardwareVaultFilter>
            {
                SearchText = SearchText,
                Filter = filter
            });

            HardwareVaults = await HardwareVaultService.GetVaultsAsync(new DataLoadingOptions<HardwareVaultFilter>
            {
                Take = TotalRecords,
                SortedColumn = nameof(HardwareVault.Id),
                SortDirection = ListSortDirection.Ascending,
                SearchText = SearchText,
                Filter = filter
            });

            var count = await WorkstationService.GetProximityVaultsCountAsync(string.Empty, WorkstationId);
            var proximityVaults = await WorkstationService.GetProximityVaultsAsync(0, count, nameof(WorkstationProximityVault.HardwareVaultId), ListSortDirection.Ascending, string.Empty, WorkstationId);
            AlreadyAdded = proximityVaults.Count > 0;

            HardwareVaults = HardwareVaults.Where(x => !proximityVaults.Select(s => s.HardwareVaultId).Contains(x.Id)).ToList();
            SelectedHardwareVault = null;
            StateHasChanged();
        }

        private async Task SelectedItemChangedAsync(HardwareVault hardwareVault)
        {
            await InvokeAsync(() =>
            {
                SelectedHardwareVault = hardwareVault;
                StateHasChanged();
            });
        }

        private async Task SearchTextChangedAsync(string searchText)
        {
            SearchText = searchText;
            await LoadDataAsync();
        }

        private async Task AddVaultAsync()
        {
            try
            {
                if (SelectedHardwareVault == null)
                {
                    WarningMessage = "Please, select a vault.";
                    return;
                }

                await WorkstationService.AddProximityVaultAsync(WorkstationId, SelectedHardwareVault.Id);
                await RemoteWorkstationConnectionsService.UpdateProximitySettingsAsync(WorkstationId, await WorkstationService.GetProximitySettingsAsync(WorkstationId));
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Vault added", ToastLevel.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.WorkstationsDetails, WorkstationId);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }
    }
}