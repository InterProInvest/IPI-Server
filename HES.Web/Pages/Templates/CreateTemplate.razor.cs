using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Templates
{
    public partial class CreateTemplate : ComponentBase
    {
        [Inject] ITemplateService TemplateService { get; set; }
        [Inject] IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] ILogger<CreateTemplate> Logger { get; set; }

        public Template Template { get; set; } = new Template();


        private async Task CreateTemplateAsync()
        {
            try
            {
                await TemplateService.CreateTmplateAsync(Template);
                ToastService.ShowToast("Employee updated.", ToastLevel.Success);
                await ModalDialogService.CloseAsync();
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
