using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Employees;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class EmployeesPage : ComponentBase
    {
        [Inject] public IMainTableService<Employee, EmployeeFilter> MainTableService { get; set; }
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }




        protected override async Task OnInitializedAsync()
        {
            await MainTableService.InitializeAsync(EmployeeService.GetEmployeesAsync, EmployeeService.GetEmployeesCountAsync, StateHasChanged, nameof(Employee.FullName));
        }


        private async Task ImportEmployeesFromAdAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddEmployee));
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Import from Ad", body);
        }

        private async Task CreateEmployeeAsync()
        {
            await MainTableService.ShowModalAsync("Details", null);
        }

        private async Task EmployeeDetailsAsync()
        {
            await InvokeAsync(() =>
            {
                NavigationManager.NavigateTo($"/Employees/Details?id={MainTableService.SelectedEntity.Id}", true);
            });
        }

        private async Task EditEmployeeAsync()
        {
            await MainTableService.ShowModalAsync("Edit", null);
        }

        private async Task DeleteEmployeeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteEmployee));
                builder.AddAttribute(1, nameof(DeleteEmployee.Employee), MainTableService.SelectedEntity);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Delete", body);
        }
    }
}
