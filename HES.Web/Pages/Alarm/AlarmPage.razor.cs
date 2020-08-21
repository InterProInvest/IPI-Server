using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using HES.Core.Models.Web.Workstations;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Alarm
{
    public partial class AlarmPage : ComponentBase, IDisposable
    {
        [Inject] public IMainTableService<Workstation, WorkstationFilter> MainTableService { get; set; }
        [Inject] public IWorkstationService WorkstationService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }

        public AlarmState AlarmState { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await BreadcrumbsService.SetAlarm();
            await GetAlarmState();
            await MainTableService.InitializeAsync(WorkstationService.GetWorkstationsAsync, WorkstationService.GetWorkstationsCountAsync, StateHasChanged, nameof(Workstation.IsOnline));
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

            await MainTableService.ShowModalAsync("Turn on alarm", body);
        }

        private async Task DisableAlarmAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DisableAlarm));
                builder.AddAttribute(1, nameof(DisableAlarm.CallBack), EventCallback.Factory.Create(this, GetAlarmState));
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Turn off alarm", body);
        }

        public void Dispose()
        {
            MainTableService.Dispose();
        }
    }
}