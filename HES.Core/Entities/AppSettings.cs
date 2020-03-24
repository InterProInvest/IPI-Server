using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities
{
    public class AppSettings
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Value { get; set; }
    }
}