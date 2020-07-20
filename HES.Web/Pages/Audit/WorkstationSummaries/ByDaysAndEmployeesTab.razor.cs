using HES.Core.Interfaces;
using HES.Core.Models;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSummaries
{
    public partial class ByDaysAndEmployeesTab : ComponentBase
    {
        [Inject] public IMainTableService<SummaryByDayAndEmployee, SummaryFilter> MainTableService { get; set; }
        [Inject] public IWorkstationAuditService WorkstationAuditService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await MainTableService.InitializeAsync(WorkstationAuditService.GetSummaryByDayAndEmployeesAsync, WorkstationAuditService.GetSummaryByDayAndEmployeesCountAsync, StateHasChanged, nameof(SummaryByDayAndEmployee.Date), syncPropName: "Date");
            await BreadcrumbsService.SetAuditSummaries();
        }   
    }
}