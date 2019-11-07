using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models
{
    public class SharedAccountDto
    {
        [Required]
        public string EmployeeId { get; set; }
        [Required]
        public string SharedAccountId { get; set; }
        [Required]
        public string[] DevicesIds { get; set; }
    }
}