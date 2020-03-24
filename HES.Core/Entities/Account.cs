using System;
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

        public bool Deleted { get; set; }

        public ushort IdFromDevice { get; set; }

        public string EmployeeId { get; set; }

        public string SharedAccountId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        [ForeignKey("SharedAccountId")]
        public SharedAccount SharedAccount { get; set; }
    }

    public enum AccountType
    {
        Personal,
        Shared
    }

    public enum AccountKind
    {
        WebApp,
        Workstation
    }
}