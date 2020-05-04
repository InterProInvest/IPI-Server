using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class HardwareVaultLicense
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        public string HardwareVaultId { get; set; }
        [Required]
        public string LicenseOrderId { get; set; }
        public DateTime? ImportedAt { get; set; }
        public DateTime? AppliedAt { get; set; }
        public DateTime? EndDate { get; set; }
        public byte[] Data { get; set; }
    }
}