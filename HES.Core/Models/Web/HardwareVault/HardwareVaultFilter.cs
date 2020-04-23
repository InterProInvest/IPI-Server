using HES.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HES.Core.Models.Web.HardwareVault
{
    public class HardwareVaultFilter
    {
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
        [Range(0, 100)]
        public int? Battery { get; set; }
        public string Firmware { get; set; }
        public VaultLicenseStatus? LicenseStatus { get; set; }
        [Display(Name = "Employee")]
        public string EmployeeId { get; set; }
        [Display(Name = "Company")]
        public string CompanyId { get; set; }
        [Display(Name = "Department")]
        public string DepartmentId { get; set; }
        public int Records { get; set; }
    }
}
