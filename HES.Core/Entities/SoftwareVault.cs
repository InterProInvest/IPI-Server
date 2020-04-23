using HES.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class SoftwareVault
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        public string OS { get; set; }
        [Required]
        public string Model { get; set; }
        [Required]
        public string ClientAppVersion { get; set; }
        public DateTime CreatedAt { get; set; }
        public VaultStatus Status { get; set; }
        [Required]
        public string EmployeeId { get; set; }
        public bool HasNewLicense { get; set; }
        public DateTime? LicenseEndDate { get; set; }
        public VaultLicenseStatus LicenseStatus { get; set; }
        public bool NeedSync { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }
    }
}