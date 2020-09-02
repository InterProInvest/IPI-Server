using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class AddProximityVaultDto
    {
        [Required]
        public string WorkstationId { get; set; }
        [Required]
        public string HardwareVaultId { get; set; }
    }
}