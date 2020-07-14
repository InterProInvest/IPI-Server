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
    public partial class DeleteWorkstation : ComponentBase
    {
        [Inject] public IWorkstationService WorkstationService { get; set; }
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<DeleteWorkstation> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public Workstation Workstation { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public async Task DeleteAsync()
        {
            try
            {
                await WorkstationService.DeleteWorkstationAsync(Workstation.Id);
                await RemoteWorkstationConnectionsService.UpdateWorkstationApprovedAsync(Workstation.Id, isApproved: false);
                ToastService.ShowToast("Workstation deleted.", ToastLevel.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Workstations);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                await ModalDialogService.CancelAsync();
                Logger.LogError(ex.Message, ex);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }
    }
}