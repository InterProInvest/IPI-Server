using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.AppSettings
{
    public class Licensing
    {
        [Required]
        public string ApiKey { get; set; }
        [Required]
        [Url]
        public string ApiAddress { get; set; }
    }
}