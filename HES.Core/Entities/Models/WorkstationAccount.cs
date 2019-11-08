using HES.Core.Entities.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities.Models
{
    public class WorkstationAccount
    {

        [RequiredIf("Skip")]
        public string Name { get; set; }

        [RequiredIf("Skip")]
        [Display(Name = "Type")]
        public WorkstationAccountType AccountType { get; set; }

        [RequiredIf("Skip")]
        public string Domain { get; set; }

        [RequiredIf("Skip")]
        [Display(Name = "User Name")]
        public string Login { get; set; }

        [RequiredIf("Skip")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [NotMapped]
        public bool Skip { get; set; }
    }

    public enum WorkstationAccountType
    {
        Local,
        Domain,
        Microsoft
    }
}