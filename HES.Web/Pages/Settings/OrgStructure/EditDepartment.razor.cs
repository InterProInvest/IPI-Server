using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.OrgStructure
{
    public partial class EditDepartment : ComponentBase, IDisposable
    {
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<EditDepartment> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public Department Department { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public bool EntityBeingEdited { get; set; }


        protected override void OnInitialized()
        {
            ModalDialogService.OnCancel += ModalDialogService_OnCancel;

            EntityBeingEdited = MemoryCache.TryGetValue(Department.Id, out object _);
            if (!EntityBeingEdited)
                MemoryCache.Set(Department.Id, Department);
        }

        private async Task EditAsync()
        {
            try
            {
                await OrgStructureService.EditDepartmentAsync(Department);
                ToastService.ShowToast("Department updated.", ToastLevel.Success);
                await Refresh.InvokeAsync(this);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.OrgSructureCompanies, Department.CompanyId);
                await ModalDialogService.CloseAsync();
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(Department.Name), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        private async Task ModalDialogService_OnCancel()
        {
            await OrgStructureService.UnchangedDepartmentAsync(Department);
            ModalDialogService.OnCancel -= ModalDialogService_OnCancel;
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Department.Id);
        }
    }
}