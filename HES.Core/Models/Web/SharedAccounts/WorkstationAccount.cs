using HES.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.SharedAccounts
{
    public class WorkstationAccount
    {

        public string Name { get; set; }

        [Display(Name = "Type")]
        public WorkstationAccountType AccountType { get; set; }

        public string Domain { get; set; }

        [Display(Name = "User Name")]
        public string Login { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

  
        public bool UpdateAdPassword { get; set; }
        public bool Skip { get; set; }
    }
}