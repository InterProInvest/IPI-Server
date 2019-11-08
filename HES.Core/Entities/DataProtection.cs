using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class DataProtection
    {
        [Key]
        public string Id { get; set; }
        public string Value { get; set; }

        [NotMapped]
        public static string PrimaryKey => "DataProtection";
    }
}