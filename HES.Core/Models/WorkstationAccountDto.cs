using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models
{
    public class WorkstationAccountDto
    {
        public WorkstationAccount WorkstationAccount { get; set; }
        [Required]
        public string EmployeeId { get; set; }
        [Required]
        public string DeviceId { get; set; }
    }
}