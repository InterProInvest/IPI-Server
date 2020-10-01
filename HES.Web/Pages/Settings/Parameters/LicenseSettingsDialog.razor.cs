using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Parameters
{
    public partial class LicenseSettingsDialog : ComponentBase
    {
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public ILogger<LicenseSettingsDialog> Logger { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ApiAddress { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        public LicensingSettings LicensingSettings { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }

        protected override void OnInitialized()
        {
            LicensingSettings = new LicensingSettings() { ApiAddress = ApiAddress };
        }

        private async Task UpdateLicensingSettingsAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await AppSettingsService.SetLicensingSettingsAsync(LicensingSettings);
                    ToastService.ShowToast("License settings updated.", ToastLevel.Success);
                    await HubContext.Clients.All.SendAsync(RefreshPage.Parameters, ConnectionId);
                    await ModalDialogService.CloseAsync();
                });
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
