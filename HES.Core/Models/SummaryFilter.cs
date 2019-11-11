using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models
{
    public class SummaryFilter
    {
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
        [Display(Name = "Employee")]
        public string EmployeeId { get; set; }
        [Display(Name = "Company")]
        public string CompanyId { get; set; }
        [Display(Name = "Department")]
        public string DepartmentId { get; set; }
        public int Records { get; set; }
    }
}