﻿using HES.Core.Models.Web.SharedAccounts;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.SharedAccounts
{
    public partial class SharedAccountFilterComponent : ComponentBase
    {
        [Parameter] public Func<SharedAccountsFilter, Task> FilterChanged { get; set; }

        public SharedAccountsFilter Filter { get; set; } = new SharedAccountsFilter();
        public ButtonSpinner ButtonSpinner { get; set; }

        private async Task FilteredAsync()
        {
            await ButtonSpinner.SpinAsync(async () =>
            {
                await FilterChanged.Invoke(Filter);
            });
        }

        private async Task ClearAsync()
        {
            Filter = new SharedAccountsFilter();
            await FilterChanged.Invoke(Filter);
        }
    }
}