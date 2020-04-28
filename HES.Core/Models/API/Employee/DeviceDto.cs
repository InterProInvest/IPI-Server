using HES.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class DeviceDto
    {
        [Required]
        public string EmployeeId { get; set; }
        [Required]
        public string DeviceId { get; set; }
        [Required]
        public VaultStatusReason Reason  { get; set; }
    }
}