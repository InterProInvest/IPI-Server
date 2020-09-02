using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API.HardwareVault
{
    public class CreateHardwareVaultProfileDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public bool ButtonBonding { get; set; }
        [Required]
        public bool ButtonConnection { get; set; }
        [Required]
        public bool ButtonNewChannel { get; set; }
        [Required]
        public bool PinBonding { get; set; }
        [Required]
        public bool PinConnection { get; set; }
        [Required]
        public bool PinNewChannel { get; set; }
        [Required]
        public bool MasterKeyConnection { get; set; }
        [Required]
        public bool MasterKeyNewChannel { get; set; }
        [Required]
        [Range(60, 172800)]
        public int PinExpiration { get; set; }
        [Required]
        [Range(4, 8)]
        public int PinLength { get; set; }
        [Required]
        [Range(3, 10)]
        public int PinTryCount { get; set; }
    }
}