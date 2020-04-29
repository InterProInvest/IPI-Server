using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hideez.SDK.Communication;

namespace HES.Core.Entities
{
    public class WorkstationSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Unlocked By")]
        public SessionSwitchSubject UnlockedBy { get; set; }

        public string WorkstationId { get; set; }

        [Display(Name = "Session")]
        public string UserSession { get; set; }

        public string HardwareVaultId { get; set; }

        public string EmployeeId { get; set; }

        public string DepartmentId { get; set; }      

        public string AccountId { get; set; }


        [ForeignKey("WorkstationId")]
        public Workstation Workstation { get; set; }

        [ForeignKey("HardwareVaultId")]
        public HardwareVault HardwareVault { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        [ForeignKey("AccountId")]
        public Account Account { get; set; }

        [NotMapped]
        public TimeSpan Duration => (EndDate ?? DateTime.UtcNow).Subtract(StartDate);
    }
}