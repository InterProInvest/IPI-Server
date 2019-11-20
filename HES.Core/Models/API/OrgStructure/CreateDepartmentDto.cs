using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class CreateDepartmentDto
    {
        [Required]
        public string CompanyId { get; set; }
        [Required]
        public string Name { get; set; }
    }
}