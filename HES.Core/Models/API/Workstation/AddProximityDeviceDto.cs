using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class AddProximityDeviceDto
    {
        [Required]
        public string WorkstationId { get; set; }
        [Required]
        public string DeviceId { get; set; }
    }
}