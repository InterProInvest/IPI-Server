using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class EditEmployee : ComponentBase
    {
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EditEmployee> Logger { get; set; }
        [Parameter] public Employee Employee { get; set; }

        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public List<Company> Companies { get; set; }
        public List<Department> Departments { get; set; }
        public List<Position> Positions { get; set; }
        public bool Initialized { get; set; }

        protected override async Task OnInitializedAsync()
        {
            ModalDialogService.OnCancel += ModalDialogService_OnCancel;
            Companies = await OrgStructureService.GetCompaniesAsync();

            if (Employee.DepartmentId == null)
            {
                Departments = new List<Department>();
            }
            else
            {
                Departments = await OrgStructureService.GetDepartmentsByCompanyIdAsync(Employee.Department.CompanyId);
            }

            Positions = await OrgStructureService.GetPositionsAsync();
            Initialized = true;
        }

        public async Task OnCompanyChangeAsync(ChangeEventArgs args)
        {
            var companyId = (string)args.Value;

            if (companyId == string.Empty)
                Employee.DepartmentId = null;

            Departments = await OrgStructureService.GetDepartmentsByCompanyIdAsync(companyId);
        }

        private async Task EditAsync()
        {
            try
            {
                await EmployeeService.EditEmployeeAsync(Employee);
                ToastService.ShowToast("Employee updated.", ToastLevel.Success);
                await ModalDialogService.CloseAsync();
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
            await EmployeeService.UnchangedEmployeeAsync(Employee);
        }
    }
}