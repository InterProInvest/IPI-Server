﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.HardwareVaults;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.HardwareVaults
{
    public partial class HardwareVaultsPage : OwningComponentBase, IDisposable
    {
        public IHardwareVaultService HardwareVaultService { get; set; }
        public IMainTableService<HardwareVault, HardwareVaultFilter> MainTableService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<HardwareVaultsPage> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Parameter] public string DashboardFilter { get; set; }

        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<HardwareVault, HardwareVaultFilter>>();

                switch (DashboardFilter)
                {
                    case "LowBattery":
                        MainTableService.DataLoadingOptions.Filter.Battery = "low";
                        break;
                    case "VaultLocked":
                        MainTableService.DataLoadingOptions.Filter.Status = VaultStatus.Locked;
                        break;
                    case "VaultReady":
                        MainTableService.DataLoadingOptions.Filter.Status = VaultStatus.Ready;
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

                await InitializeHubAsync();
                await BreadcrumbsService.SetHardwareVaults();
                await MainTableService.InitializeAsync(HardwareVaultService.GetVaultsAsync, HardwareVaultService.GetVaultsCountAsync, ModalDialogService, StateHasChanged, nameof(HardwareVault.Id));

                Initialized = true;
            }
            catch (Exception ex)
            {
                LoadFailed = true;
                ErrorMessage = ex.Message;
                Logger.LogError(ex.Message);
            }
        }

        public async Task ImportVaultsAsync()
        {
            try
            {
                await HardwareVaultService.ImportVaultsAsync();
                await MainTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync("Vaults imported.", ToastType.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }

        private async Task EditRfidAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditRfid));
                builder.AddAttribute(1, nameof(EditRfid.HardwareVaultId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(EditRfid.ConnectionId), hubConnection.ConnectionId);
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
                builder.AddAttribute(3, nameof(ChangeStatus.ConnectionId), hubConnection.ConnectionId);
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
                builder.AddAttribute(3, nameof(ChangeStatus.ConnectionId), hubConnection.ConnectionId);
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
                builder.AddAttribute(3, nameof(ChangeStatus.ConnectionId), hubConnection.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Compromised", body);
        }

        private async Task ShowActivationCodeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ShowActivationCode));
                builder.AddAttribute(1, nameof(ShowActivationCode.HardwareVaultId), MainTableService.SelectedEntity.Id);
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
                builder.AddAttribute(2, nameof(ChangeProfile.ConnectionId), hubConnection.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Profile", body);
        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On(RefreshPage.HardwareVaults, async () =>
            {
                await MainTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync("Page updated by another admin.", ToastType.Notify);
            });

            hubConnection.On<string>(RefreshPage.HardwareVaultStateChanged, async (hardwareVaultId) =>
            {
                await MainTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync($"Hardware Vault {hardwareVaultId} state changed.", ToastType.Notify);
            });

            await hubConnection.StartAsync();
        }

        public void Dispose()
        {
            if (hubConnection?.State == HubConnectionState.Connected)
                hubConnection.DisposeAsync();

            MainTableService.Dispose();
        }
    }
}