using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models
{
    public class WorkstationFilter
    {
        public string Name { get; set; }
        public string Domain { get; set; }
        [Display(Name = "Client Version")]
        public string ClientVersion { get; set; }
        [Display(Name = "Company")]
        public string CompanyId { get; set; }
        [Display(Name = "Department")]
        public string DepartmentId { get; set; }
        public string OS { get; set; }
        public string IP { get; set; }
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
        public bool? RFID { get; set; }
        public int? ProximityDevicesCount { get; set; }
        public bool? Approved { get; set; }
        public int Records { get; set; }
    }
}