using HES.Core.Entities;
using HES.Core.Enums;
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
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await MainTableService.InitializeAsync(EmployeeService.GetEmployeesAsync, EmployeeService.GetEmployeesCountAsync, StateHasChanged, nameof(Employee.FullName));
            await BreadcrumbsService.SetEmployees();
        }

        private async Task ImportEmployeesFromAdAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddEmployee));
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Import from AD", body);
        }

        private async Task CreateEmployeeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateEmployee));
                builder.CloseComponent();
            };
            await MainTableService.ShowModalAsync("Create Employee", body, ModalDialogSize.Large);
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
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditEmployee));
                builder.AddAttribute(1, nameof(DeleteEmployee.Employee), MainTableService.SelectedEntity);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit", body);
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