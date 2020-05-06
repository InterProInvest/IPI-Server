using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.Account
{
    public class WorkstationDomain
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Domain { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        public string EmployeeId { get; set; }

        public bool UpdateActiveDirectoryPassword { get; set; }
    }
}