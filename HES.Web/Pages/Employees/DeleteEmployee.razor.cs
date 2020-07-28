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

namespace HES.Web.Pages.Employees
{
    public partial class DeleteEmployee : ComponentBase, IDisposable
    {
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<DeleteEmployee> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public Employee Employee { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public bool EmployeeHasVault { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override void OnInitialized()
        {
            EntityBeingEdited = MemoryCache.TryGetValue(Employee.Id, out object _);
            if (!EntityBeingEdited)
                MemoryCache.Set(Employee.Id, Employee);
        }

        protected override void OnParametersSet()
        {
            EmployeeHasVault = Employee.HardwareVaults.Count > 0 || Employee.SoftwareVaults.Count > 0;
        }

        public async Task DeleteEmployeeAsync()
        {
            try
            {
                await EmployeeService.DeleteEmployeeAsync(Employee.Id);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Employees, Employee.Id);
                ToastService.ShowToast("Employee removed.", ToastLevel.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Employee.Id);
        }

    }
}
