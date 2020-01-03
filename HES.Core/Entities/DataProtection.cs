using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HES.Core.Crypto;

namespace HES.Core.Entities
{
    public class DataProtection
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Value { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public DataProtectionParams Params { get; set; }
    }
}