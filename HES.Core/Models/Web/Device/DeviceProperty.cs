using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models
{
    public class DeviceProperty
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string RFID { get; set; }
    }
}