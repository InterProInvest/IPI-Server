using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class AddSoftwareVault : ComponentBase
    {
        [Inject] public ISoftwareVaultService SoftwareVaultService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<AddSoftwareVault> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Parameter] public Employee Employee { get; set; }

        public ServerSettings ServerSettings { get; set; }
        public DateTime ValidTo { get; set; } = DateTime.Now;

        private bool _initialized;

        protected override async Task OnInitializedAsync()
        {
            ServerSettings = await AppSettingsService.GetServerSettingsAsync();
            _initialized = true;
        }

        private async Task SendAsync()
        {
            try
            {
                await SoftwareVaultService.CreateAndSendInvitationAsync(Employee, ServerSettings, ValidTo);         
                ToastService.ShowToast("Invitation sent.", ToastLevel.Success);
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
