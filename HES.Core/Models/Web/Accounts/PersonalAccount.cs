using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.Account
{
    public class PersonalAccount
    {
        [Required]
        public string Name { get; set; }

        public string Urls { get; set; }

        public string Apps { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string OtpSecret { get; set; }

        [Required]
        public string EmployeeId { get; set; }
    }
}