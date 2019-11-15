using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class CreateOrgStructureGenericDto
    {
        [Required]
        public string Name { get; set; }
    }
}