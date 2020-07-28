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

namespace HES.Web.Pages.Groups
{
    public partial class CreateGroup : ComponentBase
    {
        [Inject] public IGroupService GroupService { get; set; }
        [Inject] public ILogger<CreateGroup> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public Group Group = new Group();
        public ValidationErrorMessage ValidationErrorMessage { get; set; }

        private async Task CreateAsync()
        {
            try
            {     
                await GroupService.CreateGroupAsync(Group);
                ToastService.ShowToast("Group created.", ToastLevel.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Groups, null);
                await ModalDialogService.CloseAsync();
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(Core.Entities.Group.Name), ex.Message);
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