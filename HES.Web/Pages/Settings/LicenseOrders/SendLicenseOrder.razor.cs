﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public partial class SendLicenseOrder : ComponentBase
    {
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILicenseService LicenseService { get; set; }
        [Inject] public ILogger<SendLicenseOrder> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public LicenseOrder LicenseOrder { get; set; }

        private async Task SendOrderAsync()
        {
            try
            {
                await LicenseService.SendOrderAsync(LicenseOrder);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Licenses);
                ToastService.ShowToast("License order has been sent.", ToastLevel.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task CancelAsync()
        {
            await ModalDialogService.CancelAsync();
        }
    }
}
