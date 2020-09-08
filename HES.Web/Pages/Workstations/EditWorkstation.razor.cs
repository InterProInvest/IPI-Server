using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public partial class EditWorkstation : ComponentBase, IDisposable
    {
        [Inject] public IWorkstationService WorkstationService { get; set; }
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<ApproveWorkstation> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string WorkstationId { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public Workstation Workstation { get; set; }
        public List<Company> Companies { get; set; }
        public List<Department> Departments { get; set; }
        public bool Initialized { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Workstation = await WorkstationService.GetWorkstationByIdAsync(WorkstationId);

                if (Workstation == null)
                    throw new Exception("Workstation not found.");

                ModalDialogService.OnCancel += OnCancel;

                EntityBeingEdited = MemoryCache.TryGetValue(Workstation.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Workstation.Id, Workstation);

                Companies = await OrgStructureService.GetCompaniesAsync();

                if (Workstation.DepartmentId == null)
                {
                    Departments = new List<Department>();
                }
                else
                {
                    Departments = await OrgStructureService.GetDepartmentsByCompanyIdAsync(Workstation.Department.CompanyId);
                }

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task EditAsync()
        {
            try
            {
                await WorkstationService.ApproveWorkstationAsync(Workstation);
                await RemoteWorkstationConnectionsService.UpdateRfidStateAsync(Workstation.Id, Workstation.RFID);          
                ToastService.ShowToast("Workstation updated.", ToastLevel.Success);
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

        private async Task CompanyChangedAsync(ChangeEventArgs args)
        {
            Departments = await OrgStructureService.GetDepartmentsByCompanyIdAsync(args.Value.ToString());
            Workstation.DepartmentId = Departments.FirstOrDefault()?.Id;
        }

        private async Task OnCancel()
        {
            await WorkstationService.UnchangedWorkstationAsync(Workstation);
        }

        public void Dispose()
        {
            ModalDialogService.OnCancel -= OnCancel;

            if (!EntityBeingEdited)
                MemoryCache.Remove(Workstation.Id);
        }
    }
}