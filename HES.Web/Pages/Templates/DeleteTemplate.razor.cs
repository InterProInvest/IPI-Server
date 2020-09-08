using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Templates
{
    public partial class DeleteTemplate : ComponentBase, IDisposable
    {
        [Inject] public ITemplateService TemplateService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<DeleteTemplate> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public string TemplateId { get; set; }

        public Template Template { get; set; }
        public bool EntityBeingEdited { get; set; }
        public bool Initialized { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Template = await TemplateService.GetByIdAsync(TemplateId);

                if (Template == null)
                    throw new Exception("Template not found.");

                EntityBeingEdited = MemoryCache.TryGetValue(Template.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Template.Id, Template);

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task DeleteTemplateAsync()
        {
            try
            {
                await TemplateService.DeleteTemplateAsync(Template.Id);
                ToastService.ShowToast("Template deleted.", ToastLevel.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Templates, Template.Id);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Template.Id);
        }
    }
}
