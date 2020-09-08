using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.HardwareVaults;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
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
        [Inject] public NavigationManager NavigationManager { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();
            MainTableService = ScopedServices.GetRequiredService<IMainTableService<HardwareVaultProfile, HardwareVaultProfileFilter>>();

            await InitializeHubAsync();
            await MainTableService.InitializeAsync(HardwareVaultService.GetHardwareVaultProfilesAsync, HardwareVaultService.GetHardwareVaultProfileCountAsync, ModalDialogService, StateHasChanged, nameof(HardwareVaultProfile.Name), ListSortDirection.Ascending);
            await BreadcrumbsService.SetHardwareVaultProfiles();
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

            hubConnection.On<string>(RefreshPage.HardwareVaultProfiles, async (profileId) =>
            {
                if (profileId != null)
                    await HardwareVaultService.ReloadProfileAsync(profileId);

                await MainTableService.LoadTableDataAsync();
                ToastService.ShowToast("Page updated by another admin.", ToastLevel.Notify);
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
