using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.AppSettings
{
    public class Domain
    {
        [Required]
        public string IpAddress { get; set; }
    }
}