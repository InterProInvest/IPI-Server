using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API.HardwareVault
{
    public class ChangeHardwareVaultProfileDto
    {
        [Required]
        public string HardwareVaultId { get; set; }
        [Required]
        public string ProfileId { get; set; }
    }
}