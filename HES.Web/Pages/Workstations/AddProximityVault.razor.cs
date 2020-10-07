﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web;
using HES.Core.Models.Web.HardwareVaults;
using HES.Core.Models.Web.Workstations;
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
            var filter = new HardwareVaultFilter() { Status = VaultStatus.Active };
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

            var count = await WorkstationService.GetProximityVaultsCountAsync(new DataLoadingOptions<WorkstationDetailsFilter>() { EntityId = WorkstationId });
            var proximityVaultFilter = new DataLoadingOptions<WorkstationDetailsFilter>()
            {
                Take = count,
                SortedColumn = nameof(WorkstationProximityVault.HardwareVaultId),
                SortDirection = ListSortDirection.Ascending,
                EntityId = WorkstationId
            };
            var proximityVaults = await WorkstationService.GetProximityVaultsAsync(proximityVaultFilter);
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
                await ToastService.ShowToastAsync("Vault added", ToastType.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.WorkstationsDetails, WorkstationId);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }
    }
}