using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.SoftwareVault;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.SoftwareVaults
{
    public partial class VaultFilter : ComponentBase
    {
        [Inject] public ISoftwareVaultService SoftwareVaultService { get; set; }
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Parameter] public Func<SoftwareVaultFilter, Task> FilterChanged { get; set; }

        SoftwareVaultFilter Filter { get; set; } = new SoftwareVaultFilter();
        public bool Initialized { get; set; }
        public SelectList OSList { get; set; }
        public SelectList ModelList { get; set; }
        public SelectList ClientAppVersionList { get; set; }
        public SelectList StatusList { get; set; }
        public SelectList LicenseStatusList { get; set; }
        public SelectList EmployeeList { get; set; }
        public SelectList CompanyList { get; set; }
        public SelectList DepartmentList { get; set; }

        protected override async Task OnInitializedAsync()
        {
            OSList = new SelectList(await SoftwareVaultService.SoftwareVaultQuery().Select(x => x.OS).Distinct().OrderBy(x => x).ToDictionaryAsync(t => t, t => t), "Key", "Value");
            ModelList = new SelectList(await SoftwareVaultService.SoftwareVaultQuery().Select(x => x.Model).Distinct().OrderBy(x => x).ToDictionaryAsync(t => t, t => t), "Key", "Value");
            ClientAppVersionList = new SelectList(await SoftwareVaultService.SoftwareVaultQuery().Select(x => x.ClientAppVersion).Distinct().OrderBy(x => x).ToDictionaryAsync(t => t, t => t), "Key", "Value");
            StatusList = new SelectList(Enum.GetValues(typeof(VaultStatus)).Cast<VaultStatus>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            LicenseStatusList = new SelectList(Enum.GetValues(typeof(LicenseStatus)).Cast<LicenseStatus>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            EmployeeList = new SelectList(await EmployeeService.EmployeeQuery().OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToListAsync(), "Id", "FullName");
            CompanyList = new SelectList(await OrgStructureService.CompanyQuery().OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
            DepartmentList = new SelectList(new Dictionary<int, string>(), "Key", "Value");
            Initialized = true;
        }

        private async Task FilterdAsync()
        {
            await FilterChanged.Invoke(Filter);
        }

        private async Task ClearAsync()
        {
            Filter = new SoftwareVaultFilter();
            await FilterChanged.Invoke(Filter);
        }

        private async Task CompanyChangedAsync(ChangeEventArgs arg)
        {
            Filter.CompanyId = string.IsNullOrEmpty((string)arg.Value) ? null : (string)arg.Value;
            DepartmentList = new SelectList(await OrgStructureService.DepartmentQuery().Where(x => x.CompanyId == Filter.CompanyId).OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
        }
    }
}