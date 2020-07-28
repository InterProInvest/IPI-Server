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
    public partial class DeleteCompany : ComponentBase, IDisposable
    {
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<DeleteCompany> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public Company Company { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public bool EntityBeingEdited { get; set; }

        protected override void OnInitialized()
        {
            EntityBeingEdited = MemoryCache.TryGetValue(Company.Id, out object _);
            if (!EntityBeingEdited)
                MemoryCache.Set(Company.Id, Company);
        }

        public async Task DeleteAsync()
        {
            try
            {
                await OrgStructureService.DeleteCompanyAsync(Company.Id);
                await Refresh.InvokeAsync(this);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.OrgSructureCompanies, Company.Id);
                ToastService.ShowToast("Company removed.", ToastLevel.Success);
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
                MemoryCache.Remove(Company.Id);
        }
    }
}