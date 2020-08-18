using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using HES.Core.Models.Web.Workstations;
using HES.Core.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace HES.Web.Pages.Alarm
{
    public partial class AlarmPage : ComponentBase
    {
        [Inject] public IMainTableService<Workstation, WorkstationFilter> MainTableService { get; set; }
        [Inject] public IWorkstationService WorkstationService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }


        public AlarmState AlarmState { get; set; }
        public int OnlineWorkstations { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await GetAlarmState();
            OnlineWorkstations = RemoteWorkstationConnectionsService.WorkstationsOnlineCount();
            await MainTableService.InitializeAsync(WorkstationService.GetWorkstationsAsync, WorkstationService.GetWorkstationsCountAsync, StateHasChanged, nameof(Workstation.IsOnline));
        }

        private async Task GetAlarmState()
        {
            AlarmState = await AppSettingsService.GetAlarmStateAsync();
        }

        private async Task AlarmEnableAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EnableAlarm));
                builder.AddAttribute(1, nameof(EnableAlarm.CallBack), EventCallback.Factory.Create(this, GetAlarmState));
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Enable alarm", body);
        }

        private async Task DisableAlarmAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DisableAlarm));
                builder.AddAttribute(1, nameof(DisableAlarm.CallBack), EventCallback.Factory.Create(this, GetAlarmState));
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Disable alarm", body);
        }
    }
}