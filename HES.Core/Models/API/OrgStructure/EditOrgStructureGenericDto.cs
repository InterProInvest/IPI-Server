using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class EditOrgStructureGenericDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}