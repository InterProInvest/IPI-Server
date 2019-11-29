using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities
{
    public class DataProtection
    {
        [Key]
        public string Id { get; set; }
        public string Value { get; set; }
    }
}