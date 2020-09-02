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

namespace HES.Web.Pages.Workstations
{
    public partial class UnapproveWorkstation: ComponentBase, IDisposable
    {
        [Inject] public IWorkstationService WorkstationService { get; set; }
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<UnapproveWorkstation> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public Workstation Workstation { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public bool EntityBeingEdited { get; set; }

        protected override void OnInitialized()
        {
            EntityBeingEdited = MemoryCache.TryGetValue(Workstation.Id, out object _);
            if (!EntityBeingEdited)
                MemoryCache.Set(Workstation.Id, Workstation);
        }

        private async Task UnapproveAsync()
        {
            try
            {
                await WorkstationService.UnapproveWorkstationAsync(Workstation.Id);
                await RemoteWorkstationConnectionsService.UpdateWorkstationApprovedAsync(Workstation.Id, isApproved: false);
                ToastService.ShowToast("Workstation unapproved.", ToastLevel.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Workstations);
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
                MemoryCache.Remove(Workstation.Id);
        }
    }
}