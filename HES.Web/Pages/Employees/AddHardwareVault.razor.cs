using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.HardwareVault;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class AddHardwareVault : ComponentBase
    {
        [Inject] IEmployeeService EmployeeService { get; set; }
        [Inject] IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] ILogger<AddHardwareVault> Logger { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }
        [Parameter] public string EmployeeId { get; set; }

        public List<HardwareVault> HardwareVaults { get; set; }
        public HardwareVault SelectedHardwareVault { get; set; }
        public string WarningMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            SearchText = string.Empty;
            await LoadTableDataAsync();
        }

        public int TotalRecords { get; set; }
        public string SearchText { get; set; }

        private async Task LoadTableDataAsync()
        {
            TotalRecords = await HardwareVaultService.GetVaultsCountAsync(SearchText, new HardwareVaultFilter());
            HardwareVaults = await HardwareVaultService.GetVaultsAsync(0, TotalRecords, nameof(HardwareVault.Id), ListSortDirection.Ascending, SearchText, new HardwareVaultFilter());
            
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
            await LoadTableDataAsync();
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

                await EmployeeService.AddHardwareVaultAsync(EmployeeId, SelectedHardwareVault.Id);
                RemoteWorkstationConnectionsService.StartUpdateRemoteDevice(SelectedHardwareVault.Id);
                await ModalDialogService.CloseAsync();
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Vault added.", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                await ModalDialogService.CloseAsync();
                Logger.LogError(ex.Message, ex);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }

        private async Task CloseAsync()
        {
            await ModalDialogService.CloseAsync();
        }
    }
}
