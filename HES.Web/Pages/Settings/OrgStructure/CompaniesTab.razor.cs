﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.OrgStructure
{
    public partial class CompaniesTab : OwningComponentBase, IDisposable
    {
        public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<CompaniesTab> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        public List<Company> Companies { get; set; }
        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                OrgStructureService = ScopedServices.GetRequiredService<IOrgStructureService>();

                await InitializeHubAsync();
                await BreadcrumbsService.SetOrgStructure();
                await LoadCompaniesAsync();

                Initialized = true;
            }
            catch (Exception ex)
            {
                LoadFailed = true;
                ErrorMessage = ex.Message;
                Logger.LogError(ex.Message);
            }
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
                builder.AddAttribute(1, nameof(EditCompany.CompanyId), company.Id);
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
                builder.AddAttribute(1, nameof(DeleteCompany.CompanyId), company.Id);
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
                builder.AddAttribute(1, nameof(EditDepartment.DepartmentId), department.Id);
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
                builder.AddAttribute(1, nameof(DeleteDepartment.DepartmentId), department.Id);
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

            hubConnection.On(RefreshPage.OrgSructureCompanies, async () =>
            {
                await LoadCompaniesAsync();
                await ToastService.ShowToastAsync("Page updated by another admin.", ToastType.Notify);
            });

            await hubConnection.StartAsync();
        }

        public void Dispose()
        {
            if (hubConnection.State == HubConnectionState.Connected)
                hubConnection?.DisposeAsync();
        }
    }
}
