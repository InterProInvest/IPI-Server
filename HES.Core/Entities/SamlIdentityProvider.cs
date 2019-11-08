using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class SamlIdentityProvider
    {
        [Key]
        public string Id { get; set; }
        [Display(Name = "SAML IdP Authentication")]
        public bool Enabled { get; set; }
        [Required]
        [Display(Name = "SAML IdP Server URL")]
        public string Url { get; set; }

        [NotMapped]
        public static string PrimaryKey => "IdentityProvider";
        [NotMapped]
        public static string DeviceAccountName => "SAMLIdP";
    }    
}