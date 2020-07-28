using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Audit;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationEvents
{
    public partial class WorkstationEventsPage : ComponentBase
    {
        [Inject] public IMainTableService<WorkstationEvent, WorkstationEventFilter> MainTableService { get; set; }
        [Inject] public IWorkstationAuditService WorkstationAuditService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await BreadcrumbsService.SetAuditWorkstationEvents();
            await MainTableService.InitializeAsync(WorkstationAuditService.GetWorkstationEventsAsync, WorkstationAuditService.GetWorkstationEventsCountAsync, StateHasChanged, nameof(WorkstationEvent.Date), ListSortDirection.Descending);
        }
    }
}