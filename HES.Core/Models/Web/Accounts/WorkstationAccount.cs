using HES.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.Accounts
{
    public class WorkstationAccount
    {
        [Required]
        [Display(Name = "Account Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "User Logon Name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string EmployeeId { get; set; }

        public WorkstationAccountType Type { get; set; }
    }
}