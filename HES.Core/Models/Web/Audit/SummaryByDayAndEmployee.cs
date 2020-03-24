using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models
{
    public class SummaryByDayAndEmployee
    {
        public DateTime Date { get; set; }
        public string Employee { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }

        [Display(Name = "Workstations Count")]
        public int WorkstationsCount { get; set; }

        [Display(Name = "AVG Sessions Duration")]
        public TimeSpan AvgSessionsDuration { get; set; }

        [Display(Name = "Sessions Count")]
        public int SessionsCount { get; set; }

        [Display(Name = "Total Sessions Duration")]
        public TimeSpan TotalSessionsDuration { get; set; }
    }
}