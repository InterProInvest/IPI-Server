using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class EditEmployee : ComponentBase
    {
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EditEmployee> Logger { get; set; }
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Parameter] public Employee Employee { get; set; }

        public SelectList Companies { get; set; }
        public SelectList Departments { get; set; }
        public SelectList Positions { get; set; }

        public bool Initialized { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var a = await OrgStructureService.CompanyQuery().ToListAsync();
            Companies = new SelectList(a, nameof(Company.Id), nameof(Company.Name));
            
            if (Employee.Department?.CompanyId != null)
                Departments = new SelectList(await OrgStructureService.DepartmentQuery().Where(d => d.CompanyId == Employee.Department.CompanyId).ToListAsync(), nameof(Department.Id), nameof(Department.Name));

            Positions = new SelectList(await OrgStructureService.PositionQuery().ToListAsync(), nameof(Position.Id), nameof(Position.Name));

            Initialized = true;
        }

        public async Task OnCompanyChangeAsync(ChangeEventArgs args)
        {
            string companyId = (string)args.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return;

            Employee.Department.CompanyId = companyId;
            Departments = new SelectList(await OrgStructureService.DepartmentQuery().Where(d => d.CompanyId == Employee.Department.CompanyId).ToListAsync(), nameof(Department.Id), nameof(Department.Name));
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

        private async Task CloseAsync()
        {
            //await EmployeeService.UnchangedEmployeeAsync(Employee);
            await ModalDialogService.CloseAsync();
        }
    }
}
