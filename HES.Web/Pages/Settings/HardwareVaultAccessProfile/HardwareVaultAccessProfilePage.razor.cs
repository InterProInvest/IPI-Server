using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.HardwareVaults;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.HardwareVaultAccessProfile
{
    public partial class HardwareVaultAccessProfilePage : OwningComponentBase, IDisposable
    {
        public IHardwareVaultService HardwareVaultService { get; set; }
        public IMainTableService<HardwareVaultProfile, HardwareVaultProfileFilter> MainTableService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<HardwareVaultAccessProfilePage> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<HardwareVaultProfile, HardwareVaultProfileFilter>>();

                await InitializeHubAsync();
                await MainTableService.InitializeAsync(HardwareVaultService.GetHardwareVaultProfilesAsync, HardwareVaultService.GetHardwareVaultProfileCountAsync, ModalDialogService, StateHasChanged, nameof(HardwareVaultProfile.Name), ListSortDirection.Ascending);
                await BreadcrumbsService.SetHardwareVaultProfiles();

                Initialized = true;
            }
            catch (Exception ex)
            {
                LoadFailed = true;
                ErrorMessage = ex.Message;
                Logger.LogError(ex.Message);
            }
        }

        private async Task CreateProfileAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateAccessProfile));
                builder.AddAttribute(1, nameof(CreateAccessProfile.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Create Profile", body, ModalDialogSize.Default);
        }

        private async Task EditProfileAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditProfile));
                builder.AddAttribute(1, nameof(EditProfile.ConnectionId), hubConnection?.ConnectionId);
                builder.AddAttribute(2, nameof(EditProfile.HardwareVaultProfileId), MainTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit Profile", body, ModalDialogSize.Default);
        }

        private async Task DeleteProfileAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteProfile));
                builder.AddAttribute(1, nameof(DeleteProfile.ConnectionId), hubConnection?.ConnectionId);
                builder.AddAttribute(2, nameof(DeleteProfile.HardwareVaultProfileId), MainTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Delete Profile", body, ModalDialogSize.Default);
        }

        private async Task DetailsProfileAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DetailsProfile));
                builder.AddAttribute(1, nameof(DetailsProfile.AccessProfile), MainTableService.SelectedEntity);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Details Profile", body, ModalDialogSize.Default);
        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On(RefreshPage.HardwareVaultProfiles, async () =>
            {
                await MainTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync("Page updated by another admin.", ToastType.Notify);
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
