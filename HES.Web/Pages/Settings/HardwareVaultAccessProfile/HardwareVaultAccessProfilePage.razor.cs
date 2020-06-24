using HES.Core.Entities;
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
            //await InitializeHubAsync();
        }
    }
}
