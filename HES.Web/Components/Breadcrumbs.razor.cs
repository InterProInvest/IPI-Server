using HES.Core.Interfaces;
using HES.Core.Models.Web.Breadcrumb;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace HES.Web.Components
{
    public partial class Breadcrumbs : ComponentBase
    {
        [Inject] IBreadcrumbsService BreadcrumbsService { get; set; }
        public List<Breadcrumb> Items { get; set; }

        protected override void OnInitialized()
        {
            BreadcrumbsService.GetBreadcrumbs(out List<Breadcrumb> breadcrumbs);
            Items = breadcrumbs;
        }
    }
}