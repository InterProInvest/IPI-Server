using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Workstations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public partial class WorkstationPage : ComponentBase, IDisposable
    {
        [Inject] public IMainTableService<Workstation, WorkstationFilter> MainTableService { get; set; }
        [Inject] public IWorkstationService WorkstationService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Parameter] public string DashboardFilter { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            switch (DashboardFilter)
            {
                case "NotApproved":
                    MainTableService.DataLoadingOptions.Filter.Approved = false;
                    break;
                case "Online":
                    //
                    break;
            }


            await MainTableService.InitializeAsync(WorkstationService.GetWorkstationsAsync, WorkstationService.GetWorkstationsCountAsync, StateHasChanged, nameof(Workstation.Name));
            await BreadcrumbsService.SetWorkstations();
            await InitializeHubAsync();
        }

        private async Task ApproveWorkstationAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ApproveWorkstation));
                builder.AddAttribute(1, nameof(ApproveWorkstation.Workstation), MainTableService.SelectedEntity);
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
                builder.AddAttribute(1, nameof(UnapproveWorkstation.Workstation), MainTableService.SelectedEntity);
                builder.AddAttribute(2, nameof(UnapproveWorkstation.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };
            await MainTableService.ShowModalAsync("Unapprove Workstation", body);
        }

        private async Task WorkstationDetailsAsync()
        {
            await InvokeAsync(() =>
            {
                NavigationManager.NavigateTo($"/Workstations/Details?id={MainTableService.SelectedEntity.Id}", true);
            });
        }

        private async Task EditWorkstationAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditWorkstation));
                builder.AddAttribute(1, nameof(UnapproveWorkstation.Workstation), MainTableService.SelectedEntity);
                builder.AddAttribute(2, nameof(UnapproveWorkstation.ConnectionId), hubConnection?.ConnectionId);
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
                await WorkstationService.DetachWorkstationsAsync(MainTableService.Entities);
                await MainTableService.LoadTableDataAsync();
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