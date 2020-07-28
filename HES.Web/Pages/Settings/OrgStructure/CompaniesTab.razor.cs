using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.OrgStructure
{
    public partial class CompaniesTab : ComponentBase
    {
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        private HubConnection hubConnection;

        public List<Company> Companies { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadCompaniesAsync();
            await InitializeHubAsync();
            await BreadcrumbsService.SetOrgStructure();
        }

        private async Task LoadCompaniesAsync()
        {
            Companies = await OrgStructureService.GetCompaniesAsync();
            StateHasChanged();
        }

        private async Task OpenDialogCreateCompanyAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateCompany));
                builder.AddAttribute(1, nameof(CreateCompany.ConnectionId), hubConnection?.ConnectionId);
                builder.AddAttribute(2, nameof(CreateCompany.Refresh), EventCallback.Factory.Create(this, LoadCompaniesAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Create Company", body);
        }

        private async Task OpenDialogEditCompanyAsync(Company company)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditCompany));
                builder.AddAttribute(1, nameof(EditCompany.Company), company);
                builder.AddAttribute(2, nameof(EditCompany.ConnectionId), hubConnection?.ConnectionId);
                builder.AddAttribute(3, nameof(EditCompany.Refresh), EventCallback.Factory.Create(this, LoadCompaniesAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Edit Company", body);
        }

        private async Task OpenDialogDeleteCompanyAsync(Company company)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteCompany));
                builder.AddAttribute(1, nameof(DeleteCompany.Company), company);
                builder.AddAttribute(2, nameof(DeleteCompany.ConnectionId), hubConnection?.ConnectionId);
                builder.AddAttribute(3, nameof(DeleteCompany.Refresh), EventCallback.Factory.Create(this, LoadCompaniesAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete Company", body);
        }

        private async Task OpenDialogCreateDepartmentAsync(Company company)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateDepartment));
                builder.AddAttribute(1, nameof(CreateDepartment.CompanyId), company.Id);
                builder.AddAttribute(2, nameof(CreateDepartment.ConnectionId), hubConnection?.ConnectionId);
                builder.AddAttribute(3, nameof(CreateDepartment.Refresh), EventCallback.Factory.Create(this, LoadCompaniesAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Create Department", body);
        }

        private async Task OpenDialogEditDepartmentAsync(Department department)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditDepartment));
                builder.AddAttribute(1, nameof(EditDepartment.Department), department);
                builder.AddAttribute(2, nameof(EditDepartment.ConnectionId), hubConnection?.ConnectionId);
                builder.AddAttribute(3, nameof(EditDepartment.Refresh), EventCallback.Factory.Create(this, LoadCompaniesAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Edit Department", body);
        }

        private async Task OpenDialogDeleteDepartmentAsync(Department department)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteDepartment));
                builder.AddAttribute(1, nameof(DeleteDepartment.Department), department);
                builder.AddAttribute(2, nameof(DeleteDepartment.ConnectionId), hubConnection?.ConnectionId);
                builder.AddAttribute(3, nameof(DeleteDepartment.Refresh), EventCallback.Factory.Create(this, LoadCompaniesAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete Department", body);
        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On<string>(RefreshPage.OrgSructureCompanies, async (companyId) =>
            {
                if (companyId != null)
                    await OrgStructureService.ReloadCompanyAsync(companyId);

                await LoadCompaniesAsync();
                ToastService.ShowToast("Page updated by another admin.", ToastLevel.Notify);
            });

            await hubConnection.StartAsync();
        }

        public void Dispose()
        {
            _ = hubConnection.DisposeAsync();
        }
    }
}
