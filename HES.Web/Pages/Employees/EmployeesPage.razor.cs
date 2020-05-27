using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Employees;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class EmployeesPage : ComponentBase
    {
        [Inject] public IMainTableService<Employee, EmployeeFilter> MainTableService { get; set; }
        [Inject] public IEmployeeService EmployeeService { get; set; }



        protected override async Task OnInitializedAsync()
        {
            await MainTableService.InitializeAsync(EmployeeService.GetEmployeesAsync, EmployeeService.GetEmployeesCountAsync, StateHasChanged, nameof(Employee.FullName));
        }


        private async Task ImportEmployeesFromAdAsync()
        {
            await MainTableService.ShowModalAsync("Import from Ad", null);
        }
        private async Task CreateEmployeeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateEmployee));
                builder.CloseComponent();
            };
            await MainTableService.ShowModalAsync("Details", body, Core.Enums.ModalDialogSize.Large);
        }

        private async Task EmployeeDetailsAsync()
        {
            await MainTableService.ShowModalAsync("Details", null);
        }

        private async Task EditEmployeeAsync()
        {
            await MainTableService.ShowModalAsync("Edit", null);
        }

        private async Task DeleteEmployeeAsync()
        {
            await MainTableService.ShowModalAsync("Delete", null);
        }
    }
}
