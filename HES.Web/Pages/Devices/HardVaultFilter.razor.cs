using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Models.Web.HardwareVault;

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

        public SelectList Firmwares { get; set; }
        public SelectList LicenseStatuses { get; set; }
        public SelectList Employees { get; set; }
        public SelectList Companies { get; set; }

        public bool Initialized { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Filter = new HardwareVaultFilter();

            Firmwares = new SelectList(await HardwareVaultService.GetVaultsFirmwares(), "Key", "Value");
            LicenseStatuses = new SelectList(Enum.GetValues(typeof(LicenseStatus)).Cast<LicenseStatus>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            Employees = new SelectList(await EmployeeService.EmployeeQuery().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            Companies = new SelectList(await OrgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");

            Initialized = true;
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
