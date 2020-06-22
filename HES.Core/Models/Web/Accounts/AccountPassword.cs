using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models
{
    public class AccountPassword
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
 
        public bool UpdateActiveDirectoryPassword { get; set; }
    }
}