using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public partial class WorkstationDetailsPage : ComponentBase, IDisposable
    {
        [Inject] public IWorkstationService WorkstationService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Parameter] public string WorkstationId { get; set; }

        public Workstation Workstation { get; set; }
        public List<WorkstationProximityVault> WorkstationProximityVaults { get; set; }
        public WorkstationProximityVault SelectedEntity { get; set; }
        public bool Initialized { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            await LoadWorkstationAsync();
            await BreadcrumbsService.SetWorkstationDetails(Workstation.Name);
            await LoadTableDataAsync();
            await InitializeHubAsync();
            Initialized = true;
        }

        private async Task LoadWorkstationAsync()
        {
            Workstation = await WorkstationService.GetWorkstationByIdAsync(WorkstationId);
        }

        #region Main Table

        public int CurrentPage { get; set; } = 1;
        public int DisplayRows { get; set; } = 10;
        public int TotalRecords { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public string SortedColumn { get; set; } = nameof(Account.Name);
        public ListSortDirection SortDirection { get; set; } = ListSortDirection.Ascending;

        private async Task LoadTableDataAsync()
        {
            var currentTotalRows = TotalRecords;
            TotalRecords = await WorkstationService.GetProximityVaultsCountAsync(SearchText, WorkstationId);

            if (currentTotalRows != TotalRecords)
                CurrentPage = 1;

            WorkstationProximityVaults = await WorkstationService.GetProximityVaultsAsync((CurrentPage - 1) * DisplayRows, DisplayRows, SortedColumn, SortDirection, SearchText, WorkstationId);
            SelectedEntity = WorkstationProximityVaults.Contains(SelectedEntity) ? SelectedEntity : null;

            StateHasChanged();
        }

        private async Task SelectedItemChangedAsync(WorkstationProximityVault entity)
        {
            await InvokeAsync(() =>
            {
                SelectedEntity = entity;
                StateHasChanged();
            });
        }

        private async Task CurrentPageChangedAsync(int currentPage)
        {
            CurrentPage = currentPage;
            await LoadTableDataAsync();
        }

        private async Task DisplayRowsChangedAsync(int displayRows)
        {
            DisplayRows = displayRows;
            CurrentPage = 1;
            await LoadTableDataAsync();
        }

        private async Task SearchTextChangedAsync(string searchText)
        {
            SearchText = searchText;
            await LoadTableDataAsync();
        }

        private async Task SortedColumnChangedAsync(string columnName)
        {
            SortedColumn = columnName;
            await LoadTableDataAsync();
        }

        private async Task SortDirectionChangedAsync(ListSortDirection sortDirection)
        {
            SortDirection = sortDirection;
            await LoadTableDataAsync();
        }

        #endregion

        private async Task OpenDialogAddHardwareVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddProximityVault));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.AddAttribute(2, "WorkstationId", WorkstationId);
                builder.AddAttribute(3, "ConnectionId", hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add Proximity Vault", body);
        }

        private async Task OpenDialogDeleteHardwareVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteProximityVault));
                builder.AddAttribute(1, "WorkstationProximityVault", SelectedEntity);
                builder.AddAttribute(2, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.AddAttribute(3, "WorkstationId", WorkstationId);
                builder.AddAttribute(4, "ConnectionId", hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete Proximity Vault", body);
        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On<string>(RefreshPage.WorkstationsDetails, async (entityId) =>
            {
                if (entityId != WorkstationId)
                    return;

                await WorkstationService.DetachdProximityVaultsAsync(WorkstationProximityVaults);
                await LoadTableDataAsync();
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