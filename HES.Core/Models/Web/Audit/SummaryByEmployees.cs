using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models
{
    public class SummaryByEmployees
    {
        public string Employee { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }

        [Display(Name = "Workstations Count")]
        public int WorkstationsCount { get; set; }

        [Display(Name = "Working Days Count")]
        public int WorkingDaysCount { get; set; }

        [Display(Name = "Total Sessions Count")]
        public int TotalSessionsCount { get; set; }

        [Display(Name = "Total Sessions Duration")]
        public TimeSpan TotalSessionsDuration { get; set; }

        [Display(Name = "AVG Sessions Duration")]
        public TimeSpan AvgSessionsDuration { get; set; }

        [Display(Name = "AVG Sessions Count Per Day")]
        public decimal AvgSessionsCountPerDay { get; set; }
               
        [Display(Name = "AVG Working Hours Per Day")]
        public TimeSpan AvgWorkingHoursPerDay { get; set; }
    }
}