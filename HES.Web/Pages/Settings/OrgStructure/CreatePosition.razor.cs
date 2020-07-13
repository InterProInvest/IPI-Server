using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.OrgStructure
{
    public partial class CreatePosition : ComponentBase
    {
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<CreatePosition> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public Position Position { get; set; } = new Position();
        public ValidationErrorMessage ValidationErrorMessage { get; set; }

        private async Task CreateAsync()
        {
            try
            {
                await OrgStructureService.CreatePositionAsync(Position);
                ToastService.ShowToast("Position created.", ToastLevel.Success);
                await Refresh.InvokeAsync(this);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.OrgSructurePositions);
                await ModalDialogService.CloseAsync();
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(Position.Name), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
            }
        }
    }
}