using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Workstations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public partial class WorkstationDetailsPage : OwningComponentBase, IDisposable
    {
        public IWorkstationService WorkstationService { get; set; }
        public IMainTableService<WorkstationProximityVault, WorkstationDetailsFilter> MainTableService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<WorkstationDetailsPage> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Parameter] public string WorkstationId { get; set; }

        public Workstation Workstation { get; set; }
        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationService = ScopedServices.GetRequiredService<IWorkstationService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<WorkstationProximityVault, WorkstationDetailsFilter>>();

                await InitializeHubAsync();
                await LoadWorkstationAsync();
                await BreadcrumbsService.SetWorkstationDetails(Workstation.Name);
                await MainTableService.InitializeAsync(WorkstationService.GetProximityVaultsAsync, WorkstationService.GetProximityVaultsCountAsync, ModalDialogService, StateHasChanged, nameof(WorkstationProximityVault.HardwareVaultId), entityId: WorkstationId);
                Initialized = true;
            }
            catch (Exception ex)
            {
                LoadFailed = true;
                ErrorMessage = ex.Message;
                Logger.LogError(ex.Message);
            }
        }

        private async Task LoadWorkstationAsync()
        {
            Workstation = await WorkstationService.GetWorkstationByIdAsync(WorkstationId);
            if (Workstation == null)
                throw new Exception("Workstation not found.");
        }

        private async Task OpenDialogAddHardwareVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddProximityVault));
                builder.AddAttribute(1, "WorkstationId", WorkstationId);
                builder.AddAttribute(2, "ConnectionId", hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add Proximity Vault", body);
        }

        private async Task OpenDialogDeleteHardwareVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteProximityVault));
                builder.AddAttribute(1, "WorkstationProximityVault", MainTableService.SelectedEntity);
                builder.AddAttribute(2, "WorkstationId", WorkstationId);
                builder.AddAttribute(3, "ConnectionId", hubConnection?.ConnectionId);
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

                await MainTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync("Page updated by another admin.", ToastType.Notify);
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
