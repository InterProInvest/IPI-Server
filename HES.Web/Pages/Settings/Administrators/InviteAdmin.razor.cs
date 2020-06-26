using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Administrators
{
    public partial class InviteAdmin : ComponentBase
    {
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IEmailSenderService EmailSenderService { get; set; }
        [Inject] public IApplicationUserService ApplicationUserService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<InviteAdmin> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }

        [Parameter] public string ConnectionId { get; set; }

        public string InvitedEmail { get; set; } = string.Empty;

        private async Task InviteAdminAsync()
        {
            try
            {
                var callBakcUrl = await ApplicationUserService.InviteAdministratorAsync(InvitedEmail, NavigationManager.BaseUri);
                await EmailSenderService.SendUserInvitationAsync(InvitedEmail, callBakcUrl);
                ToastService.ShowToast("Administrator invited.", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
            finally
            {
                await HubContext.Clients.All.SendAsync(RefreshPage.Administrators, ConnectionId);
                await ModalDialogService.CloseAsync();
            }
        }
    }
}
