using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class CreateTemplateDto
    {
        [Required]
        public string Name { get; set; }
        public string Urls { get; set; }
        public string Apps { get; set; }
    }
}