using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Alarm
{
    public partial class EnableAlarm : ComponentBase
    {
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnections { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EnableAlarm> Logger { get; set; }

        [Parameter] public EventCallback CallBack { get; set; }

        private async Task EnableAlarmAsync()
        {
            var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var applicationUser = await UserManager.GetUserAsync(state.User);

            try
            {
                await RemoteWorkstationConnections.LockAllWorkstations(applicationUser);
                ToastService.ShowToast("All workstations are locked.", ToastLevel.Success);
                
                await CallBack.InvokeAsync(this);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }
    }
}