﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public partial class DeleteLicenseOrder : ComponentBase, IDisposable 
    {
        [Inject] public ILicenseService LicenseService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<DeleteLicenseOrder> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public LicenseOrder LicenseOrder { get; set; }

        public bool EntityBeingEdited { get; set; }

        protected override void OnInitialized()
        {
            EntityBeingEdited = MemoryCache.TryGetValue(LicenseOrder.Id, out object _);
            if (!EntityBeingEdited)
                MemoryCache.Set(LicenseOrder.Id, LicenseOrder);
        }

        private async Task DeleteOrderAsync()
        {
            try
            {
                await LicenseService.DeleteOrderAsync(LicenseOrder);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Licenses, LicenseOrder.Id);
                ToastService.ShowToast("License order deleted.", ToastLevel.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(LicenseOrder.Id);
        }
    }
}