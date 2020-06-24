using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.HardwareVaults;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.HardwareVaultAccessProfile
{
    public partial class HardwareVaultAccessProfilePage : ComponentBase
    {
        [Inject] public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMainTableService<HardwareVaultProfile, HardwareVaultProfileFilter> MainTableService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            await MainTableService.InitializeAsync(HardwareVaultService.GetHardwareVaultProfilesAsync, HardwareVaultService.GetHardwareVaultProfileCountAsync, StateHasChanged, nameof(HardwareVaultProfile.Name), ListSortDirection.Descending);
            await BreadcrumbsService.SetHardwareVaultProfiles();
            await InitializeHubAsync();
        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On<string>(RefreshPage.HardwareVaultProfiles, async (connectionId) =>
            {
                var id = hubConnection.ConnectionId;
                if (id != connectionId)
                {
                    await HardwareVaultService.DetachProfilesAsync(MainTableService.Entities);
                    await MainTableService.LoadTableDataAsync();
                    StateHasChanged();
                    ToastService.ShowToast("Page updated by another admin.", ToastLevel.Notify);
                }
            });

            await hubConnection.StartAsync();
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
            return;
        }

        private async Task DeleteProfileAsync()
        {
            return;
        }
    }
}
