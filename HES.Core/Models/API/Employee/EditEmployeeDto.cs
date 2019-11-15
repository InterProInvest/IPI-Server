using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class EditEmployeeDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        [Required]
        public string DepartmentId { get; set; }
        [Required]
        public string PositionId { get; set; }
    }
}