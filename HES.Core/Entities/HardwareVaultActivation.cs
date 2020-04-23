using HES.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class HardwareVaultActivation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        public string VaultId { get; set; }
        [Required]
        public string AcivationCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public HardwareVaultActivationStatus Status { get; set; }
    }
}