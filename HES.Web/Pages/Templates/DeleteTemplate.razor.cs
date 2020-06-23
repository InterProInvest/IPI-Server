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
        [Inject] public IHubContext<TemplatesHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public Template Template { get; set; }

        private async Task DeleteTemplateAsync()
        {
            try
            {
                await TemplateService.DeleteTemplateAsync(Template.Id);
                ToastService.ShowToast("Accounts template deleted.", ToastLevel.Success);
                await HubContext.Clients.All.SendAsync("PageUpdated", ConnectionId);
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
