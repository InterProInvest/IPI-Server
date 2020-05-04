using HES.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class SoftwareVaultInvitation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        public string EmployeeId { get; set; }
        public InviteVaultStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ValidTo { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public string SoftwareVaultId { get; set; }
        public int ActivationCode { get; set; }


        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        [ForeignKey("SoftwareVaultId")]
        public SoftwareVault SoftwareVault { get; set; }
    }
}