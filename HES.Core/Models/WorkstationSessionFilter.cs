using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models
{
    public class WorkstationSessionFilter
    {
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
        [Display(Name = "Unlocked by")]
        public int? UnlockId { get; set; }
        [Display(Name = "Workstation")]
        public string WorkstationId { get; set; }
        [Display(Name = "Session")]
        public string UserSession { get; set; }
        [Display(Name = "Device")]
        public string DeviceId { get; set; }
        [Display(Name = "Employee")]
        public string EmployeeId { get; set; }
        [Display(Name = "Company")]
        public string CompanyId { get; set; }
        [Display(Name = "Department")]
        public string DepartmentId { get; set; }
        [Display(Name = "Account")]
        public string DeviceAccountId { get; set; }
        [Display(Name = "Account Type")]
        public int? DeviceAccountTypeId { get; set; }
        public int Records { get; set; }
    }
}