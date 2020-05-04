using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hideez.SDK.Communication;

namespace HES.Core.Entities
{
    public class WorkstationEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public DateTime Date { get; set; }
        [Display(Name = "Event")]
        public WorkstationEventType EventId { get; set; }
        [Display(Name = "Severity")]
        public WorkstationEventSeverity SeverityId { get; set; }
        public string Note { get; set; }
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
    }    
}