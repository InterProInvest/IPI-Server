using HES.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API.Employee
{
    public class RemoveHardwareVaultDto
    {
        [Required]
        public string HardwareVaultId { get; set; }
        [Required]
        public VaultStatusReason Reason { get; set; }
        public bool IsNeedBackup { get; set; }
    }
}