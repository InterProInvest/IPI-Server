using HES.Core.Interfaces;
using HES.Core.Models.Web.Audit;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSummaries
{
    public partial class ByDepartmentsTab : ComponentBase, IDisposable
    {
        [Inject] public IMainTableService<SummaryByDepartments, SummaryFilter> MainTableService { get; set; }
        [Inject] public IWorkstationAuditService WorkstationAuditService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await MainTableService.InitializeAsync(WorkstationAuditService.GetSummaryByDepartmentsAsync, WorkstationAuditService.GetSummaryByDepartmentsCountAsync, StateHasChanged, nameof(SummaryByDepartments.Company), syncPropName: "Company");
        }

        public void Dispose()
        {
            MainTableService.Dispose();
        }
    }
}