using Microsoft.AspNetCore.Components;

namespace HES.Web.Pages.Settings.OrgStructure
{
    public partial class OrgStructurePage : ComponentBase
    {
        public RenderFragment Tab { get; set; }

        protected override void  OnInitialized()
        {
            GetCompaniesTab();
        }

        private void GetCompaniesTab()
        {
            Tab = (builder) =>
            {
                builder.OpenComponent(0, typeof(CompaniesTab));
                builder.CloseComponent();
            };
        }

        private void GetPositionsTab()
        {
            Tab = (builder) =>
            {
                builder.OpenComponent(0, typeof(PositionsTab));
                builder.CloseComponent();
            };
        }
    }
}