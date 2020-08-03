using HES.Core.Interfaces;
using HES.Core.Models.Web.Audit;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSummaries
{
    public partial class ByDaysAndEmployeesTab : ComponentBase, IDisposable
    {
        [Inject] public IMainTableService<SummaryByDayAndEmployee, SummaryFilter> MainTableService { get; set; }
        [Inject] public IWorkstationAuditService WorkstationAuditService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await MainTableService.InitializeAsync(WorkstationAuditService.GetSummaryByDayAndEmployeesAsync, WorkstationAuditService.GetSummaryByDayAndEmployeesCountAsync, StateHasChanged, nameof(SummaryByDayAndEmployee.Date), syncPropName: "Date");
        }

        public void Dispose()
        {
            MainTableService.Dispose();
        }
    }
}