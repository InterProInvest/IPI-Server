using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class DeleteEmployee : ComponentBase
    {
        [Inject] IEmployeeService EmployeeService { get; set; }
        [Inject] IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] ILogger<DeleteEmployee> Logger { get; set; }
        [Parameter] public Employee Employee { get; set; }

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
                await ModalDialogService.CloseAsync();
                ToastService.ShowToast("Employee removed.", ToastLevel.Success);
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
