using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Audit;
using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSessions
{
    public partial class WorkstationSessionsPage : ComponentBase
    {
        [Inject] public IMainTableService<WorkstationSession, WorkstationSessionFilter> MainTableService { get; set; }
        [Inject] public IWorkstationAuditService WorkstationAuditService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Parameter] public string DashboardFilter { get; set; }

        protected override async Task OnInitializedAsync()
        {
            switch (DashboardFilter)
            {
                case "NonHideezUnlock":
                    MainTableService.DataLoadingOptions.Filter.UnlockedBy = Hideez.SDK.Communication.SessionSwitchSubject.NonHideez;
                    break;
                case "LongOpenSession":
                    MainTableService.DataLoadingOptions.Filter.Query = WorkstationAuditService.SessionQuery().Where(x => x.StartDate <= DateTime.UtcNow.AddHours(-12) && x.EndDate == null);
                    break;
                case "OpenedSessions":
                    MainTableService.DataLoadingOptions.Filter.Query = WorkstationAuditService.SessionQuery().Where(x => x.EndDate == null);
                    break;
            }

            await BreadcrumbsService.SetAuditWorkstationSessions();
            await MainTableService.InitializeAsync(WorkstationAuditService.GetWorkstationSessionsAsync, WorkstationAuditService.GetWorkstationSessionsCountAsync, StateHasChanged, nameof(WorkstationSession.StartDate), ListSortDirection.Descending);
        }
    }
}