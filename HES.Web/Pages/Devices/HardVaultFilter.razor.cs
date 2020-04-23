using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Models.Web.HardwareVault;
using HES.Core.Entities;
using System.Collections.Generic;
using HES.Core.Models.Web.Breadcrumb;
using Microsoft.JSInterop;

namespace HES.Web.Pages.Devices
{
    public partial class HardVaultFilter : ComponentBase
    {
        [Inject]
        public IEmployeeService EmployeeService { get; set; }

        [Inject]
        public IDeviceService HardwareVaultService { get; set; }

        [Inject]
        public IOrgStructureService OrgStructureService { get; set; }


        [Parameter] 
        public Func<HardwareVaultFilter, Task> FilterChanged { get; set; }

        public HardwareVaultFilter Filter { get; set; }
        public SelectList LicenseStatuses { get; set; }

        

        protected override async Task OnInitializedAsync()
        {
            Filter = new HardwareVaultFilter();
            LicenseStatuses = new SelectList(Enum.GetValues(typeof(VaultLicenseStatus)).Cast<VaultLicenseStatus>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
        }

        private async Task FilteredAsync()
        {
            await FilterChanged.Invoke(Filter);
        }

        private async Task ClearAsync()
        {
            Filter = new HardwareVaultFilter();
            await FilterChanged.Invoke(Filter);
        }
    }
}
