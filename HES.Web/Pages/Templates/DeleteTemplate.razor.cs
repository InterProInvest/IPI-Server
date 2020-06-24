using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Templates
{
    public partial class DeleteTemplate : ComponentBase
    {
        [Inject] public ITemplateService TemplateService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<DeleteTemplate> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public Template Template { get; set; }

        private async Task DeleteTemplateAsync()
        {
            try
            {
                await TemplateService.DeleteTemplateAsync(Template.Id);
                ToastService.ShowToast("Template deleted.", ToastLevel.Success);
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
