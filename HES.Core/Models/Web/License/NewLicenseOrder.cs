using HES.Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.License
{
    public class NewLicenseOrder
    {
        [Required]
        [EmailAddress]
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