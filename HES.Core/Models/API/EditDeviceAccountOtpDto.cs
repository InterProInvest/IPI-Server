using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class EditDeviceAccountOtpDto
    {
        [Required]
        public string Id { get; set; }
        public string OtpSercret { get; set; }
        [Required]
        public string DeviceId { get; set; }
    }
}