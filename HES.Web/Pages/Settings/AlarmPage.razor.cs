using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using HES.Core.Models.Web.Workstations;
using HES.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings
{
    public partial class AlarmPage : ComponentBase
    {
        [Inject] public ILogger<AlarmPage> Logger { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IHubContext<AppHub> HubContext { get; set; }
        [Inject] public IMainTableService<Workstation, WorkstationFilter> MainTableService { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public IWorkstationService WorkstationService { get; set; }
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnections { get; set; }

        [Inject] public IAppSettingsService AppSettingsService { get; set; }


        public AlarmState AlarmState { get; set; }
        public int OnlineWorkstations { get; set; }


        private ApplicationUser _applicationUser;
        protected override async Task OnInitializedAsync()
        {
            var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _applicationUser = await UserManager.GetUserAsync(state.User);

            AlarmState = await AppSettingsService.GetAlarmStateAsync();
            OnlineWorkstations = RemoteWorkstationConnectionsService.WorkstationsOnlineCount();
            await MainTableService.InitializeAsync(WorkstationService.GetWorkstationsAsync, WorkstationService.GetWorkstationsCountAsync, StateHasChanged, nameof(Workstation.IsOnline));
        }

        private async Task BlockAllWorkstationsAsync()
        {
            try
            {
                AlarmState = await RemoteWorkstationConnections.LockAllWorkstations(_applicationUser);
                ToastService.ShowToast("All workstations are successfully locked", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }

        private async Task UnlockAllWorkstationsAsync()
        {
            try
            {
                await RemoteWorkstationConnections.UnlockAllWorkstations(_applicationUser);
                AlarmState = null;
                ToastService.ShowToast("All workstations are successfully unlocked", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }
    }
}