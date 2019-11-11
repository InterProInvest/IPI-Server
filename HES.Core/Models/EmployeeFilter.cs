using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models
{
    public class EmployeeFilter
    {
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
        [Display(Name = "Company")]
        public string CompanyId { get; set; }
        [Display(Name = "Department")]
        public string DepartmentId { get; set; }
        [Display(Name = "Position")]
        public string PositionId { get; set; }
        [Display(Name = "Devices Count")]
        public int? DevicesCount { get; set; }
        public int Records { get; set; }
    }
}