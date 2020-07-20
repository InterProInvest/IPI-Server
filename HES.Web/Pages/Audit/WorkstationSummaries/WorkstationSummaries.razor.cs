using Microsoft.AspNetCore.Components;

namespace HES.Web.Pages.Audit.WorkstationSummaries
{
    public partial class WorkstationSummaries : ComponentBase
    {
        public RenderFragment Tab { get; set; }

        protected override void OnInitialized()
        {
            GetByDaysAndEmployeesTab();
        }

        private void GetByDaysAndEmployeesTab()
        {
            Tab = (builder) =>
            {
                builder.OpenComponent(0, typeof(ByDaysAndEmployeesTab));
                builder.CloseComponent();
            };
        }

        private void GetByEmployeesTab()
        {
            Tab = (builder) =>
            {
                builder.OpenComponent(0, typeof(ByEmployeesTab));
                builder.CloseComponent();
            };
        }

        private void GetByDepartmentsTab()
        {
            Tab = (builder) =>
            {
                builder.OpenComponent(0, typeof(ByDepartmentsTab));
                builder.CloseComponent();
            };
        }

        private void GetByWorkstationsTab()
        {
            Tab = (builder) =>
            {
                builder.OpenComponent(0, typeof(ByWorkstationsTab));
                builder.CloseComponent();
            };
        }
    }
}