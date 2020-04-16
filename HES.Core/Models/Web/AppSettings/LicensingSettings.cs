using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.AppSettings
{
    public class LicensingSettings
    {
        [Required]
        public string ApiKey { get; set; }
        [Required]
        [Url]
        public string ApiAddress { get; set; }
    }
}