using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.AppSettings
{
    public class DomainSettings
    {
        [Required]
        public string Host { get; set; }
    }
}