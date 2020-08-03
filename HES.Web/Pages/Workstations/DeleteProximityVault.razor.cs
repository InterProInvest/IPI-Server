using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public partial class DeleteProximityVault : ComponentBase
    {
        [Inject] IWorkstationService WorkstationService { get; set; }
        [Inject] IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] ILogger<DeleteProximityVault> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public WorkstationProximityVault WorkstationProximityVault { get; set; }
        [Parameter] public string WorkstationId { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public async Task DeleteVaultAsync()
        {
            try
            {   
                await WorkstationService.DeleteProximityVaultAsync(WorkstationProximityVault.Id);          
                ToastService.ShowToast("Vault deleted.", ToastLevel.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.WorkstationsDetails, WorkstationId);
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