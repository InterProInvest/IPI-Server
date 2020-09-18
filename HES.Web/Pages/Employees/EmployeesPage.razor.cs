using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Employees;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class EmployeesPage : OwningComponentBase, IDisposable
    {
        public IEmployeeService EmployeeService { get; set; }
        public IMainTableService<Employee, EmployeeFilter> MainTableService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
            MainTableService = ScopedServices.GetRequiredService<IMainTableService<Employee, EmployeeFilter>>();

            await InitializeHubAsync();
            await BreadcrumbsService.SetEmployees();
            await MainTableService.InitializeAsync(EmployeeService.GetEmployeesAsync, EmployeeService.GetEmployeesCountAsync, ModalDialogService, StateHasChanged, nameof(Employee.FullName));
        }

        //private async Task ImportEmployeesFromAdAsync()
        //{
        //    RenderFragment body = (builder) =>
        //    {
        //        builder.OpenComponent(0, typeof(AddEmployee));
        //        builder.AddAttribute(1, "ConnectionId", hubConnection?.ConnectionId);
        //        builder.CloseComponent();
        //    };

        //    await MainTableService.ShowModalAsync("Import from AD", body);
        //}

        private async Task SyncEmployeesWithAdAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SyncEmployeesWithAD));
                builder.AddAttribute(1, nameof(SyncEmployeesWithAD.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Sync with Active Directory", body, ModalDialogSize.Large);
        }

        private async Task EmployeeDetailsAsync()
        {
            await InvokeAsync(() =>
            {
                NavigationManager.NavigateTo($"/Employees/Details/{MainTableService.SelectedEntity.Id}");
            });
        }

        private async Task CreateEmployeeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateEmployee));
                builder.AddAttribute(1, "ConnectionId", hubConnection?.ConnectionId);
                builder.CloseComponent();
            };
            await MainTableService.ShowModalAsync("Create Employee", body, ModalDialogSize.Large);
        }

        private async Task EditEmployeeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditEmployee));
                builder.AddAttribute(1, nameof(DeleteEmployee.EmployeeId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(DeleteEmployee.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit Employee", body);
        }

        private async Task DeleteEmployeeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteEmployee));
                builder.AddAttribute(1, nameof(DeleteEmployee.EmployeeId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(DeleteEmployee.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Delete Employee", body);
        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On(RefreshPage.Employees, async () =>
            {
                await MainTableService.LoadTableDataAsync();
                ToastService.ShowToast("Page updated by another admin.", ToastLevel.Notify);
            });

            await hubConnection.StartAsync();
        }

        public void Dispose()
        {
            if (hubConnection?.State == HubConnectionState.Connected)
                hubConnection.DisposeAsync();

            MainTableService.Dispose();
        }
    }
}