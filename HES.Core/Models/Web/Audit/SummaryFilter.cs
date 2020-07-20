using System;

namespace HES.Core.Models
{
    public class SummaryFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Employee { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        public int Records { get; set; }
    }
}