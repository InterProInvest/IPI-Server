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

namespace HES.Web.Pages.Templates
{
    public partial class EditTemplate : ComponentBase
    {
        [Inject] public ITemplateService TemplateService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EditTemplate> Logger { get; set; }
        [Inject] public IHubContext<TemplatesHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public Template Template { get; set; }

        public ValidationErrorMessage ValidationErrorMessage { get; set; }

        protected override void OnInitialized()
        {
            ModalDialogService.OnCancel += CancelAsync;
        }

        private async Task EditTemplateAsync()
        {
            try
            {
                await TemplateService.EditTemplateAsync(Template);
                ToastService.ShowToast("Template updated.", ToastLevel.Success);
                await HubContext.Clients.All.SendAsync("PageUpdated", ConnectionId);
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
    }
}
