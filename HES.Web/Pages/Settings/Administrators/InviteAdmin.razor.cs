using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppUsers;
using HES.Web.Components;
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

        public Invitation Invitation = new Invitation();
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }

        private async Task InviteAdminAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    var callBakcUrl = await ApplicationUserService.InviteAdministratorAsync(Invitation.Email, NavigationManager.BaseUri);
                    await EmailSenderService.SendUserInvitationAsync(Invitation.Email, callBakcUrl);
                    ToastService.ShowToast("Administrator invited.", ToastLevel.Success);
                    await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Administrators);
                    await ModalDialogService.CloseAsync();
                });
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(Invitation.Email), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }  
        }
    }
}