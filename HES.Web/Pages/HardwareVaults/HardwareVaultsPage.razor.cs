using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Breadcrumb;
using HES.Core.Models.Web.HardwareVault;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.HardwareVaults
{
    public partial class HardwareVaultsPage : ComponentBase
    {
        [Inject] public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] ILogger<HardwareVaultsPage> Logger { get; set; }
        [Parameter] public string DashboardFilter { get; set; }

        public List<HardwareVault> HardwareVaults { get; set; }
        public HardwareVault SelectedHardwareVault { get; set; }

        public HardwareVaultFilter Filter { get; set; } = new HardwareVaultFilter();
        public string SearchText { get; set; } = string.Empty;
        public ListSortDirection SortDirection { get; set; } = ListSortDirection.Ascending;
        public string SortedColumn { get; set; } = nameof(HardwareVault.Id);
        public int DisplayRows { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
        public int TotalRecords { get; set; }

        protected override async Task OnInitializedAsync()
        {
            switch (DashboardFilter)
            {
                case "LowBattery":
                    Filter.Battery = "low";
                    break;
                case "VaultLocked":
                    Filter.VaultStatus = VaultStatus.Locked;
                    break;
                case "VaultReady":
                    Filter.VaultStatus = VaultStatus.Ready;
                    break;
                case "LicenseWarning":
                    Filter.LicenseStatus = VaultLicenseStatus.Warning;
                    break;
                case "LicenseCritical":
                    Filter.LicenseStatus = VaultLicenseStatus.Critical;
                    break;
                case "LicenseExpired":
                    Filter.LicenseStatus = VaultLicenseStatus.Expired;
                    break;
            }
            await LoadTableDataAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var items = new List<Breadcrumb>()
                {
                    new Breadcrumb () { Active = true, Content = "Hardware Vaults" }
                };
                await JSRuntime.InvokeVoidAsync("createBreadcrumbs", items);
            }
        }

        #region Main Table

        private async Task LoadTableDataAsync()
        {
            var currentTotalRows = TotalRecords;
            TotalRecords = await HardwareVaultService.GetVaultsCountAsync(SearchText, Filter);

            if (currentTotalRows != TotalRecords)
                CurrentPage = 1;

            HardwareVaults = await HardwareVaultService.GetVaultsAsync((CurrentPage - 1) * DisplayRows, DisplayRows, SortedColumn, SortDirection, SearchText, Filter);
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

        private async Task SortedColumnChangedAsync(string columnName)
        {
            SortedColumn = columnName;
            await LoadTableDataAsync();
        }

        private async Task SortDirectionChangedAsync(ListSortDirection sortDirection)
        {
            SortDirection = sortDirection;
            await LoadTableDataAsync();
        }

        private async Task CurrentPageChangedAsync(int currentPage)
        {
            CurrentPage = currentPage;
            await LoadTableDataAsync();
        }

        private async Task DisplayRowsChangedAsync(int displayRows)
        {
            DisplayRows = displayRows;
            CurrentPage = 1;
            await LoadTableDataAsync();
        }

        private async Task SearchTextChangedAsync(string searchText)
        {
            SearchText = searchText;
            await LoadTableDataAsync();
        }

        private async Task FilterChangedAsync(HardwareVaultFilter filter)
        {
            Filter = filter;
            await LoadTableDataAsync();
        }

        #endregion


        #region Table Actions

        public async Task ImportVaultsAsync()
        {
            try
            {
                await HardwareVaultService.ImportVaultsAsync();
                await LoadTableDataAsync();
                ToastService.ShowToast("Vaults imported.", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast("Somethings went wrong.", ToastLevel.Error);
            }
        }

        private async Task EditRfidAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditRfid));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.AddAttribute(2, "HardwareVaultId", SelectedHardwareVault.Id);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Edit RFID", body);
        }

        private async Task SuspendVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ChangeStatus));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.AddAttribute(2, "HardwareVaultId", SelectedHardwareVault.Id);
                builder.AddAttribute(3, "VaultStatus", VaultStatus.Suspended);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Suspend", body);
        }

        private async Task ActivateVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ChangeStatus));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.AddAttribute(2, "HardwareVaultId", SelectedHardwareVault.Id);
                builder.AddAttribute(3, "VaultStatus", VaultStatus.Active);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Activate", body);
        }

        private async Task CompromisedVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ChangeStatus));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.AddAttribute(2, "HardwareVaultId", SelectedHardwareVault.Id);
                builder.AddAttribute(3, "VaultStatus", VaultStatus.Compromised);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Compromised", body);
        }

        private async Task ShowActivationCodeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ShowActivationCode));
                builder.AddAttribute(1, "HardwareVault", SelectedHardwareVault);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Activation code", body);
        }

        private async Task ChangeVaultProfileAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ChangeProfile));
                builder.AddAttribute(1, "HardwareVaultId", SelectedHardwareVault.Id);
                builder.AddAttribute(2, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Profile", body);
        }

        #endregion
    }
}
