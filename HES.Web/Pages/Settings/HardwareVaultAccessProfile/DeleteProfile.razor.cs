using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.HardwareVaultAccessProfile
{
    public partial class DeleteProfile : ComponentBase
    {
        [Inject] public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<DeleteProfile> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public HardwareVaultProfile AccessProfile { get; set; }

        private async Task DeleteProfileAsync()
        {
            try
            {
                await HardwareVaultService.DeleteProfileAsync(AccessProfile.Id);
                ToastService.ShowToast("Hardware vault profile deleted.", ToastLevel.Success);
                await HubContext.Clients.All.SendAsync(RefreshPage.Templates, ConnectionId);
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
