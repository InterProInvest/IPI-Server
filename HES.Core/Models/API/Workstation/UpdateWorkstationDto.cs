using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class UpdateWorkstationDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string DepartmentId { get; set; }
        [Required]
        public bool RfidEnabled { get; set; }
    }
}