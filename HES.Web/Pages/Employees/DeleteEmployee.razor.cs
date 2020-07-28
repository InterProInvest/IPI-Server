using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class DeleteEmployee : ComponentBase
    {
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<DeleteEmployee> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public Employee Employee { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public bool EmployeeHasVault { get; set; }


        protected override void OnParametersSet()
        {
            EmployeeHasVault = Employee.HardwareVaults.Count > 0 || Employee.SoftwareVaults.Count > 0;
        }

        public async Task DeleteEmployeeAsync()
        {
            try
            {
                await EmployeeService.DeleteEmployeeAsync(Employee.Id);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Employees, null);
                ToastService.ShowToast("Employee removed.", ToastLevel.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                await ModalDialogService.CloseAsync();
                Logger.LogError(ex.Message, ex);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }

        public async Task CancelAsync()
        {
            await ModalDialogService.CloseAsync();
        }
    }
}
