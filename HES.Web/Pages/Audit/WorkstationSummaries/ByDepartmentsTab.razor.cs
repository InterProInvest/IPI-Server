using HES.Core.Interfaces;
using HES.Core.Models.Web.Audit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSummaries
{
    public partial class ByDepartmentsTab : OwningComponentBase, IDisposable
    {
        public IWorkstationAuditService WorkstationAuditService { get; set; }
        [Inject] public IMainTableService<SummaryByDepartments, SummaryFilter> MainTableService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            WorkstationAuditService = ScopedServices.GetRequiredService<IWorkstationAuditService>();
            await MainTableService.InitializeAsync(WorkstationAuditService.GetSummaryByDepartmentsAsync, WorkstationAuditService.GetSummaryByDepartmentsCountAsync, StateHasChanged, nameof(SummaryByDepartments.Company), syncPropName: "Company");
        }

        public void Dispose()
        {
            WorkstationAuditService.Dispose();
            MainTableService.Dispose();
        }
    }
}