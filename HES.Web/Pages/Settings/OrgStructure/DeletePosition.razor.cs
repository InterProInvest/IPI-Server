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


namespace HES.Web.Pages.Settings.OrgStructure
{
    public partial class DeletePosition : ComponentBase, IDisposable
    {
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<DeletePosition> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public Position Position { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public bool EntityBeingEdited { get; set; }

        protected override void OnInitialized()
        {
            EntityBeingEdited = MemoryCache.TryGetValue(Position.Id, out object _);
            if (!EntityBeingEdited)
                MemoryCache.Set(Position.Id, Position);
        }

        public async Task DeleteAsync()
        {
            try
            {
                await OrgStructureService.DeletePositionAsync(Position.Id);
                await Refresh.InvokeAsync(this);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.OrgSructurePositions, Position.Id);
                ToastService.ShowToast("Position removed.", ToastLevel.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                await ModalDialogService.CancelAsync();
                Logger.LogError(ex.Message, ex);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Position.Id);
        }
    }
}