using System;
using HES.Core.Enums;
using HES.Core.Entities;
using HES.Core.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace HES.Web.Pages.Alarm
{
    public partial class DisableAlarm : ComponentBase
    {
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<DisableAlarm> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnections { get; set; }

        [Parameter] public EventCallback CallBack { get; set; }

        public string UserConfirmPassword { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        protected override async Task OnInitializedAsync()
        {
            UserConfirmPassword = string.Empty;
            var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            ApplicationUser = await UserManager.GetUserAsync(state.User);
        }

        private async Task DisableAlarmAsync()
        {
            var checkPassword = await UserManager.CheckPasswordAsync(ApplicationUser, UserConfirmPassword);

            if (!checkPassword)
            {
                UserConfirmPassword = string.Empty;
                ToastService.ShowToast("Invalid password", ToastLevel.Error);
                await ModalDialogService.CloseAsync();
                return;
            }

            try
            {
                await RemoteWorkstationConnections.UnlockAllWorkstations(ApplicationUser);
                ToastService.ShowToast("All workstations are unlocked.", ToastLevel.Success);

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