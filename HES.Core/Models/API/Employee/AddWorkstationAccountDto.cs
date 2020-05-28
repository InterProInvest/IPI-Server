using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API.Employee
{
    public class AddWorkstationAccountDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string EmployeeId { get; set; }
    }
}