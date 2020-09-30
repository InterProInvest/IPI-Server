using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.Accounts
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
        [CompareProperty("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string OtpSecret { get; set; }

        public bool UpdateInActiveDirectory { get; set; }

        [Required]
        public string EmployeeId { get; set; }
    }
}