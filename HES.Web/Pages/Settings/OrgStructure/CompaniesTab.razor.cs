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
            await InitializeHubAsync();
            await LoadCompaniesAsync();
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

        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On(RefreshPage.OrgSructureCompanies, async () =>
            {
                //await EmployeeService.DetachEmployeeAsync(MainTableService.Entities);
                //await MainTableService.LoadTableDataAsync();
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
