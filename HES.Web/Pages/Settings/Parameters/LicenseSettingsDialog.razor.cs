﻿using HES.Core.Enums;
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
        [Parameter] public LicensingSettings LicensingSettings { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }

        public string InputType { get; private set; }

        protected override void OnInitialized()
        {
            InputType = "Password";
        }

        private async Task UpdateLicensingSettingsAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await AppSettingsService.SetLicensingSettingsAsync(LicensingSettings);
                    await ToastService.ShowToastAsync("License settings updated.", ToastType.Success);
                    await HubContext.Clients.All.SendAsync(RefreshPage.Parameters, ConnectionId);
                    await ModalDialogService.CloseAsync();
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CloseAsync();
            }
        }
    }
}
