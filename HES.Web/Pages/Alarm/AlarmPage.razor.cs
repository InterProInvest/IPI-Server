using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web;
using HES.Core.Models.Web.AppSettings;
using HES.Core.Models.Web.Workstations;
using HES.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Alarm
{
    public partial class AlarmPage : OwningComponentBase, IDisposable
    {
        public IWorkstationService WorkstationService { get; set; }
        public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        public AlarmState AlarmState { get; set; }
        public int WorkstationOnline { get; set; }
        public int WorkstationCount { get; set; }

        private HubConnection _hubConnection;

        protected override async Task OnInitializedAsync()
        {
            WorkstationService = ScopedServices.GetRequiredService<IWorkstationService>();
            AppSettingsService = ScopedServices.GetRequiredService<IAppSettingsService>();

            await BreadcrumbsService.SetAlarm();
            await GetAlarmStateAsync();
            await InitializeHubAsync();
            WorkstationOnline = RemoteWorkstationConnectionsService.WorkstationsOnlineCount();
            WorkstationCount = await WorkstationService.GetWorkstationsCountAsync(new DataLoadingOptions<WorkstationFilter>());
        }

        private async Task GetAlarmStateAsync()
        {
            AlarmState = await AppSettingsService.GetAlarmStateAsync();

            if (AlarmState == null)
                AlarmState = new AlarmState();
        }

        private async Task EnableAlarmAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EnableAlarm));
                builder.AddAttribute(1, nameof(EnableAlarm.CallBack), EventCallback.Factory.Create(this, GetAlarmStateAsync));
                builder.AddAttribute(2, nameof(EnableAlarm.ConnectionId), _hubConnection.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Turn on alarm", body);
        }

        private async Task DisableAlarmAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DisableAlarm));
                builder.AddAttribute(1, nameof(DisableAlarm.CallBack), EventCallback.Factory.Create(this, GetAlarmStateAsync));
                builder.AddAttribute(2, nameof(DisableAlarm.ConnectionId), _hubConnection.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Turn off alarm", body);
        }

        private async Task InitializeHubAsync()
        {
            _hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            _hubConnection.On(RefreshPage.Alarm, async () =>
            {
                await GetAlarmStateAsync();
                StateHasChanged();
                ToastService.ShowToast("Page updated by another admin.", ToastLevel.Notify);
            });

            await _hubConnection.StartAsync();
        }

        public void Dispose()
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
                _hubConnection.DisposeAsync();
        }
    }
}