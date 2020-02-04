using HES.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class LicenseOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; }
        public string Note { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public bool ProlongExistingLicenses { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public OrderStatus OrderStatus { get; set; } = OrderStatus.New;
    }
}