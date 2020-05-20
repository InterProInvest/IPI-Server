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
        public string Password { get; set; }
        public string OtpSecret { get; set; }

    }
}