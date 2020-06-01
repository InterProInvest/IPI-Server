using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.License;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public partial class LicenseOrdersPage : ComponentBase
    {
        [Inject] public IMainTableService<LicenseOrder, LicenseOrderFilter> MainTableService { get; set; }

        [Inject] public ILicenseService LicenseService { get; set; }


        protected override async Task OnInitializedAsync()
        {
            await MainTableService.InitializeAsync(LicenseService.GetLicenseOrdersAsync, LicenseService.GetLicenseOrdersCountAsync, StateHasChanged, nameof(LicenseOrder.CreatedAt));
        }

    }
}
