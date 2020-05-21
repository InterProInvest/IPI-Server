using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class AddSharedAccountDto
    {
        [Required]
        public string EmployeeId { get; set; }
        [Required]
        public string SharedAccountId { get; set; }
    }
}