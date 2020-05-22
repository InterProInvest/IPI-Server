using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API.Employee
{
    public class AddHardwareVaultDto
    {
        [Required]
        public string EmployeeId { get; set; }
        [Required]
        public string HardwareVaultId { get; set; }
    }
}