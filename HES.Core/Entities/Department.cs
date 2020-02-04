using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class Department
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        [Display(Name = "Company")]
        public string CompanyId { get; set; }
        [Required]
        [Display(Name = "Departments")]
        public string Name { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
    }
}