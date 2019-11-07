using HES.Core.Entities;
using HES.Core.Entities.Models;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models
{
    public class DeviceAccountDto
    {
        public DeviceAccount DeviceAccount { get; set; }
        public AccountPassword AccountPassword { get; set; }
        [Required]
        public string[] DevicesIds { get; set; }
    }
}