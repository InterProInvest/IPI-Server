using HES.Core.Entities;
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
    public partial class DeleteAdministrator : ComponentBase
    {
        [Inject] public IApplicationUserService ApplicationUserService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<DeleteAdministrator> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ApplicationUser = await ApplicationUserService.GetByIdAsync(ApplicationUserId);
                if (ApplicationUser == null)
                    throw new Exception("User not found.");
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private async Task DeleteUserAsync()
        {
            try
            {
                await ApplicationUserService.DeleteUserAsync(ApplicationUserId);
                await ToastService.ShowToastAsync("Administrator deleted.", ToastType.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Administrators);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }
    }
}