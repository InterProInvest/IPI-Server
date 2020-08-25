using HES.Core.Interfaces;
using HES.Core.Models.Web;
using HES.Core.Models.Web.AppSettings;
using HES.Core.Models.Web.Workstations;
using HES.Core.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace HES.Web.Pages.Alarm
{
    public partial class AlarmPage : ComponentBase
    {
        [Inject] public IWorkstationService WorkstationService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }

        public AlarmState AlarmState { get; set; }
        public int WorkstationOnline { get; set; }
        public int WorkstationCount { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await BreadcrumbsService.SetAlarm();
            await GetAlarmState();
            WorkstationOnline = RemoteWorkstationConnectionsService.WorkstationsOnlineCount();
            WorkstationCount = await WorkstationService.GetWorkstationsCountAsync(new DataLoadingOptions<WorkstationFilter>());
        }

        private async Task GetAlarmState()
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
                builder.AddAttribute(1, nameof(EnableAlarm.CallBack), EventCallback.Factory.Create(this, GetAlarmState));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Turn on alarm", body);
        }

        private async Task DisableAlarmAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DisableAlarm));
                builder.AddAttribute(1, nameof(DisableAlarm.CallBack), EventCallback.Factory.Create(this, GetAlarmState));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Turn off alarm", body);
        }
    }
}