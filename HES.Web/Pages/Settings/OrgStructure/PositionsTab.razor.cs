using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.OrgStructure
{
    public partial class PositionsTab : OwningComponentBase, IDisposable
    {
        public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<PositionsTab> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        public List<Position> Positions { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public bool IsSortedAscending { get; set; } = true;
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
                await LoadPositionsAsync();

                Initialized = true;
            }
            catch (Exception ex)
            {
                LoadFailed = true;
                ErrorMessage = ex.Message;
                Logger.LogError(ex.Message);
            }
        }

        private string GetSortIcon()
        {
            if (IsSortedAscending)
            {
                return "table-sort-arrow-up";
            }
            else
            {
                return "table-sort-arrow-down";
            }
        }

        private void SortTable()
        {
            IsSortedAscending = !IsSortedAscending;

            if (IsSortedAscending)
            {
                Positions = Positions.OrderBy(x => x.Name).ToList();
            }
            else
            {
                Positions = Positions.OrderByDescending(x => x.Name).ToList();
            }
        }

        private async Task LoadPositionsAsync()
        {
            Positions = await OrgStructureService.GetPositionsAsync();
            StateHasChanged();
        }

        private async Task OpenDialogCreatePositionAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreatePosition));
                builder.AddAttribute(1, nameof(CreatePosition.ConnectionId), hubConnection?.ConnectionId);
                builder.AddAttribute(2, nameof(CreatePosition.Refresh), EventCallback.Factory.Create(this, LoadPositionsAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Create Position", body);
        }

        private async Task OpenDialogEditPositionAsync(Position position)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditPosition));
                builder.AddAttribute(1, nameof(EditPosition.PositionId), position.Id);
                builder.AddAttribute(2, nameof(EditPosition.ConnectionId), hubConnection?.ConnectionId);
                builder.AddAttribute(3, nameof(EditPosition.Refresh), EventCallback.Factory.Create(this, LoadPositionsAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Edit Position", body);
        }

        private async Task OpenDialogDeletePositionAsync(Position position)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeletePosition));
                builder.AddAttribute(1, nameof(DeletePosition.PositionId), position.Id);
                builder.AddAttribute(2, nameof(DeletePosition.ConnectionId), hubConnection?.ConnectionId);
                builder.AddAttribute(3, nameof(DeletePosition.Refresh), EventCallback.Factory.Create(this, LoadPositionsAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete Position", body);
        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On(RefreshPage.OrgSructurePositions, async () =>
            {
                await LoadPositionsAsync();
                ToastService.ShowToast("Page updated by another admin.", ToastLevel.Notify);
            });

            await hubConnection.StartAsync();
        }

        public void Dispose()
        {
            if (hubConnection?.State == HubConnectionState.Connected)
                hubConnection.DisposeAsync();
        }
    }
}
