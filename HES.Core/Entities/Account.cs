using HES.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Urls { get; set; }

        public string Apps { get; set; }

        [Required]
        public string Login { get; set; }

        public AccountType Type { get; set; }

        public AccountKind Kind { get; set; }

        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "PasswordUpdated")]
        public DateTime PasswordUpdatedAt { get; set; }

        [Display(Name = "OtpUpdated")]
        public DateTime? OtpUpdatedAt { get; set; }

        public string Password { get; set; }

        public string OtpSecret { get; set; }

        public bool UpdateInActiveDirectory { get; set; }

        public bool Deleted { get; set; }

        [Required]
        public byte[] StorageId { get; set; }

        public uint Timestamp { get; set; }

        public string EmployeeId { get; set; }

        public string SharedAccountId { get; set; }

        public List<WorkstationEvent> WorkstationEvents { get; set; }
        public List<WorkstationSession> WorkstationSessions { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        [ForeignKey("SharedAccountId")]
        public SharedAccount SharedAccount { get; set; }
    }
}