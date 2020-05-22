using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class CreateEmployeeDto
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string DepartmentId { get; set; }
        public string PositionId { get; set; }
    }
}