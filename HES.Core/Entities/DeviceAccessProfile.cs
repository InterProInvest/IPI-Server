using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class DeviceAccessProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "Updated")]
        public DateTime? UpdatedAt { get; set; }
        public List<Device> Devices { get; set; }
        public bool ButtonBonding { get; set; }
        public bool ButtonConnection { get; set; }
        public bool ButtonNewChannel { get; set; }
        public bool PinBonding { get; set; }
        public bool PinConnection { get; set; }
        public bool PinNewChannel { get; set; }
        public bool MasterKeyBonding { get; set; } = true;
        public bool MasterKeyConnection { get; set; }
        public bool MasterKeyNewChannel { get; set; }
        public int PinExpiration { get; set; }
        public int PinLength { get; set; }
        public int PinTryCount { get; set; }

        /// <summary>
        /// logic min value: 1, max value: 107
        /// minutes: 1-59
        /// hours: 60-107 -> (value - 59) = hrs
        /// </summary>
        [NotMapped]
        public int PinExpirationConverted
        {
            get
            {
                var prop = PinExpiration / 60;
                return prop <= 59 ? prop : (prop / 60) + 59;
            }
            set
            {
                PinExpiration = value <= 59 ? value * 60 : (value - 59) * 3600;
            }
        }

        [NotMapped]
        public string PinExpirationString
        {
            get
            {
                var prop = PinExpiration / 60;
                return prop <= 59 ? ($"{prop} min") : ($"{(prop / 60)} hrs");
            }

        }
    }
}