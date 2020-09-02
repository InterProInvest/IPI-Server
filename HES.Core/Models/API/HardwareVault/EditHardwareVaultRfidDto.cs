using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API.HardwareVault
{
    public class EditHardwareVaultRfidDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string RFID { get; set; }
    }
}