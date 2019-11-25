using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class EditDepartmentDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string CompanyId { get; set; }
        [Required]
        public string Name { get; set; }
    }
}