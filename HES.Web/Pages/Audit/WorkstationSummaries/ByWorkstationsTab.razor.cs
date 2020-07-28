using HES.Core.Interfaces;
using HES.Core.Models.Web.Audit;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSummaries
{
    public partial class ByWorkstationsTab : ComponentBase
    {
        [Inject] public IMainTableService<SummaryByWorkstations, SummaryFilter> MainTableService { get; set; }
        [Inject] public IWorkstationAuditService WorkstationAuditService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await MainTableService.InitializeAsync(WorkstationAuditService.GetSummaryByWorkstationsAsync, WorkstationAuditService.GetSummaryByWorkstationsCountAsync, StateHasChanged, nameof(SummaryByWorkstations.Workstation), syncPropName: "Workstation");
        }
    }
}