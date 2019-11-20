using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class SetDeviceAccessProfileDto
    {
        [Required]
        public string DeviceId { get; set; }
        [Required]
        public string ProfileId { get; set; }
    }
}