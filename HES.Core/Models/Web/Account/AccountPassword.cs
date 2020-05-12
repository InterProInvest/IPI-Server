using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models
{
    public class AccountPassword
    {
        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirm password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "OTP secret")]
        public string OtpSecret { get; set; }
        public bool UpdateActiveDirectoryPassword { get; set; }
    }
}