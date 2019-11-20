using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class EditDeviceDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string RFID { get; set; }
    }
}