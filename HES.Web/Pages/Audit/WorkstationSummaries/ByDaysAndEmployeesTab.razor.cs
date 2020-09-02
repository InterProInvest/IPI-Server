using HES.Core.Interfaces;
using HES.Core.Models.Web.Audit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSummaries
{
    public partial class ByDaysAndEmployeesTab : OwningComponentBase, IDisposable
    {
        public IWorkstationAuditService WorkstationAuditService { get; set; }
        [Inject] public IMainTableService<SummaryByDayAndEmployee, SummaryFilter> MainTableService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            WorkstationAuditService = ScopedServices.GetRequiredService<IWorkstationAuditService>();
            await MainTableService.InitializeAsync(WorkstationAuditService.GetSummaryByDayAndEmployeesAsync, WorkstationAuditService.GetSummaryByDayAndEmployeesCountAsync, StateHasChanged, nameof(SummaryByDayAndEmployee.Date), syncPropName: "Date");
        }

        public void Dispose()
        {
            WorkstationAuditService.Dispose();
            MainTableService.Dispose();
        }
    }
}