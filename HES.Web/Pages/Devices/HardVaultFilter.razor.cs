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
        public SelectList Departaments { get; set; }

        public bool Initialized { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Filter = new HardwareVaultFilter();

            Firmwares = new SelectList(await HardwareVaultService.GetVaultsFirmwares(), "Key", "Value");
            LicenseStatuses = new SelectList(Enum.GetValues(typeof(VaultLicenseStatus)).Cast<VaultLicenseStatus>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            Employees = new SelectList(await EmployeeService.EmployeeQuery().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), nameof(Employee.Id), nameof(Employee.FullName));
            Companies = new SelectList(await OrgStructureService.CompanyQuery().ToListAsync(), nameof(Company.Id), nameof(Company.Name));

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

        private async Task CompanyOnChangeAsync(ChangeEventArgs args)
        {
            Filter.CompanyId = (string)args.Value;

            if (string.IsNullOrWhiteSpace(Filter.CompanyId))
            {
                Departaments = null;
                return;
            }

            Departaments = new SelectList(await OrgStructureService.GetDepartmentsByCompanyIdAsync(Filter.CompanyId), nameof(Department.Id), nameof(Department.Name));
            StateHasChanged();
        }
    }
}
