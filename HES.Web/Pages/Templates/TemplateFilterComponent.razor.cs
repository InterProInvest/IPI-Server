using HES.Core.Interfaces;
using HES.Core.Models.Web.Accounts;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Templates
{
    public partial class TemplateFilterComponent : ComponentBase
    {
        [Parameter] public Func<TemplateFilter, Task> FilterChanged { get; set; }

        public TemplateFilter Filter { get; set; } = new TemplateFilter();


        private async Task FilteredAsync()
        {
            await FilterChanged.Invoke(Filter);
        }

        private async Task ClearAsync()
        {
            Filter = new TemplateFilter();
            await FilterChanged.Invoke(Filter);
        }
    }
}
