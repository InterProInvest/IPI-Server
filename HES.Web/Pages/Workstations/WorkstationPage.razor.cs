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
    public partial class WorkstationPage : OwningComponentBase, IDisposable
    {
        public IWorkstationService WorkstationService { get; set; }
        public IMainTableService<Workstation, WorkstationFilter> MainTableService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ILogger<WorkstationPage> Logger { get; set; }
        [Parameter] public string DashboardFilter { get; set; }

        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationService = ScopedServices.GetRequiredService<IWorkstationService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<Workstation, WorkstationFilter>>();

                switch (DashboardFilter)
                {
                    case "NotApproved":
                        MainTableService.DataLoadingOptions.Filter.Approved = false;
                        break;
                    case "Online":
                        //
                        break;
                }

                await BreadcrumbsService.SetWorkstations();
                await MainTableService.InitializeAsync(WorkstationService.GetWorkstationsAsync, WorkstationService.GetWorkstationsCountAsync, ModalDialogService, StateHasChanged, nameof(Workstation.Name));
                await InitializeHubAsync();

                Initialized = true;
            }
            catch (Exception ex)
            {
                LoadFailed = true;
                ErrorMessage = ex.Message;
                Logger.LogError(ex.Message);
            }
        }

        private async Task ApproveWorkstationAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ApproveWorkstation));
                builder.AddAttribute(1, nameof(ApproveWorkstation.WorkstationId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(ApproveWorkstation.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };
            await MainTableService.ShowModalAsync("Approve Workstation", body);
        }

        private async Task UnapproveWorkstationAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(UnapproveWorkstation));
                builder.AddAttribute(1, nameof(UnapproveWorkstation.WorkstationId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(UnapproveWorkstation.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };
            await MainTableService.ShowModalAsync("Unapprove Workstation", body);
        }

        private async Task DeleteWorkstationAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteWorkstation));
                builder.AddAttribute(1, nameof(DeleteWorkstation.WorkstationId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(DeleteWorkstation.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };
            await MainTableService.ShowModalAsync("Delete Workstation", body);
        }

        private async Task WorkstationDetailsAsync()
        {
            await InvokeAsync(() =>
            {
                NavigationManager.NavigateTo($"/Workstations/Details/{MainTableService.SelectedEntity.Id}");
            });
        }

        private async Task EditWorkstationAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditWorkstation));
                builder.AddAttribute(1, nameof(EditWorkstation.WorkstationId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(EditWorkstation.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };
            await MainTableService.ShowModalAsync("Edit Workstation", body);
        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On(RefreshPage.Workstations, async () =>
            {
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