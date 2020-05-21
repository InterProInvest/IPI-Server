using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class EditAccountOtpDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string EmployeeId { get; set; }
        public string OtpSercret { get; set; }
    }
}