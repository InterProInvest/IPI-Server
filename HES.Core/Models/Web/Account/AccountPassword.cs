using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models
{
    public class AccountPassword
    {
        [Required]       
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "OTP secret")]
        public string OtpSecret { get; set; }
    }
}