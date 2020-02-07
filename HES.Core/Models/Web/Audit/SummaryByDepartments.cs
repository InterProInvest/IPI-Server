using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models
{
    public class SummaryByDepartments
    {
        public string Company { get; set; }
        public string Department { get; set; }

        [Display(Name = "Employees Count")]
        public int EmployeesCount { get; set; }

        [Display(Name = "Workstations Count")]
        public int WorkstationsCount { get; set; }

        [Display(Name = "Total Sessions Count")]
        public int TotalSessionsCount { get; set; }

        [Display(Name = "Total Sessions Duration")]
        public TimeSpan TotalSessionsDuration { get; set; }

        [Display(Name = "AVG Sessions Duration")]
        public TimeSpan AvgSessionsDuration { get; set; }

        [Display(Name = "AVG Total Duartion By Employee")]
        public TimeSpan AvgTotalDuartionByEmployee { get; set; }

        [Display(Name = "AVG Total Sessions Count By Employee")]
        public decimal AvgTotalSessionsCountByEmployee { get; set; }
    }
}