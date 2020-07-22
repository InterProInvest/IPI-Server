using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Audit;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSessions
{
    public partial class WorkstationSessionsPage : ComponentBase
    {
        [Inject] public IMainTableService<WorkstationSession, WorkstationSessionFilter> MainTableService { get; set; }
        [Inject] public IWorkstationAuditService WorkstationAuditService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await BreadcrumbsService.SetAuditWorkstationSessions();
            await MainTableService.InitializeAsync(WorkstationAuditService.GetWorkstationSessionsAsync, WorkstationAuditService.GetWorkstationSessionsCountAsync, StateHasChanged, nameof(WorkstationSession.StartDate), ListSortDirection.Descending);
        }
    }
}