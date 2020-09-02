using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Audit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationEvents
{
    public partial class WorkstationEventsPage : OwningComponentBase, IDisposable
    {
        public IWorkstationAuditService WorkstationAuditService { get; set; }
        [Inject] public IMainTableService<WorkstationEvent, WorkstationEventFilter> MainTableService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            WorkstationAuditService = ScopedServices.GetRequiredService<IWorkstationAuditService>();

            await BreadcrumbsService.SetAuditWorkstationEvents();
            await MainTableService.InitializeAsync(WorkstationAuditService.GetWorkstationEventsAsync, WorkstationAuditService.GetWorkstationEventsCountAsync, StateHasChanged, nameof(WorkstationEvent.Date), ListSortDirection.Descending);
        }

        public void Dispose()
        {
            WorkstationAuditService.Dispose();
            MainTableService.Dispose();
        }
    }
}