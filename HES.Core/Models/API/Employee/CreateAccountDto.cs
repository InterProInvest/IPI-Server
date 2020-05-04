using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class CreateAccountDto
    {
        [Required]
        public string EmployeeId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Urls { get; set; }
        public string Apps { get; set; }
        [Required]
        public string Login { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        public string Password { get; set; }
        public string OtpSecret { get; set; }

    }
}