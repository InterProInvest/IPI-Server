using HES.Core.Interfaces;
using HES.Core.Models.Web.Audit;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSummaries
{
    public partial class ByEmployeesTab : ComponentBase
    {
        [Inject] public IMainTableService<SummaryByEmployees, SummaryFilter> MainTableService { get; set; }
        [Inject] public IWorkstationAuditService WorkstationAuditService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await MainTableService.InitializeAsync(WorkstationAuditService.GetSummaryByEmployeesAsync, WorkstationAuditService.GetSummaryByEmployeesCountAsync, StateHasChanged, nameof(SummaryByEmployees.Employee), syncPropName: "Employee");
        }   
    }
}