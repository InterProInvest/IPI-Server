using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Templates
{
    public partial class EditTemplate : ComponentBase, IDisposable
    {
        [Inject] public ITemplateService TemplateService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<EditTemplate> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public Template Template { get; set; }

        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override void OnInitialized()
        {
            ModalDialogService.OnCancel += CancelAsync;

            EntityBeingEdited = MemoryCache.TryGetValue(Template.Id, out object _);
            if (!EntityBeingEdited)
                MemoryCache.Set(Template.Id, Template);
        }

        private async Task EditTemplateAsync()
        {
            try
            {
                await TemplateService.EditTemplateAsync(Template);
                ToastService.ShowToast("Template updated.", ToastLevel.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Templates, Template.Id);
                await ModalDialogService.CloseAsync();
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(SharedAccount.Name), ex.Message);
            }
            catch (IncorrectUrlException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(SharedAccount.Urls), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await CancelAsync();
            }
        }

        private async Task CancelAsync()
        {
            await TemplateService.UnchangedTemplateAsync(Template);
            ModalDialogService.OnCancel -= CancelAsync;
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Template.Id);
        }
    }
}
