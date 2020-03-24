using HES.Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.License
{
    public class NewLicenseOrder
    {
        [Required]
        [RegularExpression(@"^[a-z0-9][-a-z0-9.!#$%&'*+-=?^_`{|}~\/]+@([-a-z0-9]+\.)+[a-z]{2,5}$", ErrorMessage = "The Email field is not a valid e-mail address.")]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; }
        public string Note { get; set; }
        [Required]
        [Display(Name = "Start Date")]
        [NotLessThanCurrentDate]
        public DateTime StartDate { get; set; }
        [Required]
        [Display(Name = "End Date")]
        [DateMustNotBeLessThan("StartDate")]
        public DateTime EndDate { get; set; }
    }
}