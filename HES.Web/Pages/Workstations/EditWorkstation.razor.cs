using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public partial class EditWorkstation : ComponentBase
    {
        [Inject] public IWorkstationService WorkstationService { get; set; }
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<ApproveWorkstation> Logger { get; set; }
        [Inject] public IHubContext<WorkstationsHub> HubContext { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Parameter] public Workstation Workstation { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public List<Company> Companies { get; set; }
        public List<Department> Departments { get; set; }
        public bool Initialized { get; set; }

        protected override async Task OnInitializedAsync()
        {
            ModalDialogService.OnCancel += OnCancel;
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

        private async Task EditAsync()
        {
            try
            {
                await WorkstationService.ApproveWorkstationAsync(Workstation);
                await RemoteWorkstationConnectionsService.UpdateRfidStateAsync(Workstation.Id, Workstation.RFID);          
                ToastService.ShowToast("Workstation updated.", ToastLevel.Success);
                await HubContext.Clients.All.SendAsync("PageUpdated", ConnectionId);
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
            ModalDialogService.OnCancel -= OnCancel;
        }
    }
}