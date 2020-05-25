using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Breadcrumb;
using HES.Core.Models.Web.HardwareVault;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.HardwareVaults
{
    public partial class HardwareVaultsPage : ComponentBase
    {
        [Inject] public IMainTableService<HardwareVault, HardwareVaultFilter> MainTableService { get; set; }

        [Inject] public IHardwareVaultService HardwareVaultService { get; set; }

        [Inject] IToastService ToastService { get; set; }
        [Inject] ILogger<HardwareVaultsPage> Logger { get; set; }

        //TODO Переделать при полном переходе на blazor
        [Parameter] public string DashboardFilter { get; set; }
        protected override async Task OnInitializedAsync()
        {
            switch (DashboardFilter)
            {
                case "LowBattery":
                    MainTableService.DataLoadingOptions.Filter.Battery = "low";
                    break;
                case "VaultLocked":
                    MainTableService.DataLoadingOptions.Filter.VaultStatus = VaultStatus.Locked;
                    break;
                case "VaultReady":
                    MainTableService.DataLoadingOptions.Filter.VaultStatus = VaultStatus.Ready;
                    break;
                case "LicenseWarning":
                    MainTableService.DataLoadingOptions.Filter.LicenseStatus = VaultLicenseStatus.Warning;
                    break;
                case "LicenseCritical":
                    MainTableService.DataLoadingOptions.Filter.LicenseStatus = VaultLicenseStatus.Critical;
                    break;
                case "LicenseExpired":
                    MainTableService.DataLoadingOptions.Filter.LicenseStatus = VaultLicenseStatus.Expired;
                    break;
            }
            MainTableService.Initialize(HardwareVaultService.GetVaultsAsync, HardwareVaultService.GetVaultsCountAsync, StateHasChanged, nameof(HardwareVault.Id));
            await MainTableService.LoadTableDataAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            await MainTableService.InvokeJsAsync("createBreadcrumbs", new Breadcrumb[] { new Breadcrumb { Active = true, Content = "Hardware Vaults" } });
        }

        public async Task ImportVaultsAsync()
        {
            try
            {
                await HardwareVaultService.ImportVaultsAsync();
                await MainTableService.LoadTableDataAsync();
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
                builder.AddAttribute(1, nameof(EditRfid.HardwareVaultId), MainTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit RFID", body);
        }

        private async Task SuspendVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ChangeStatus));
                builder.AddAttribute(1, nameof(ChangeStatus.HardwareVaultId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(ChangeStatus.VaultStatus), VaultStatus.Suspended);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Suspend", body);
        }

        private async Task ActivateVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ChangeStatus));
                builder.AddAttribute(1, nameof(ChangeStatus.HardwareVaultId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(ChangeStatus.VaultStatus), VaultStatus.Active);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Activate", body);
        }

        private async Task CompromisedVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ChangeStatus));
                builder.AddAttribute(1, nameof(ChangeStatus.HardwareVaultId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(ChangeStatus.VaultStatus), VaultStatus.Compromised);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Compromised", body);
        }

        private async Task ShowActivationCodeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ShowActivationCode));
                builder.AddAttribute(1, nameof(ShowActivationCode.HardwareVault), MainTableService.SelectedEntity);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Activation code", body);
        }

        private async Task ChangeVaultProfileAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ChangeProfile));
                builder.AddAttribute(1, nameof(ChangeProfile.HardwareVaultId), MainTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Profile", body);
        }
    }
}
