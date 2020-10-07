﻿using HES.Core.Models.Web.Audit;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSummaries
{
    public partial class SummaryFilterComponent : ComponentBase
    {
        [Parameter] public Func<SummaryFilter, Task> FilterChanged { get; set; }
        [Parameter] public string TabName { get; set; }

        public SummaryFilter Filter { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }

        protected override void OnInitialized()
        {
            Filter = new SummaryFilter();
        }

        private async Task FilteredAsync()
        {
            await ButtonSpinner.SpinAsync(async () =>
            {
                await FilterChanged.Invoke(Filter);
            });
        }

        private async Task ClearAsync()
        {
            Filter = new SummaryFilter();
            await FilterChanged.Invoke(Filter);
        }
    }
}