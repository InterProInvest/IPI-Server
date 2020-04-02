using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.AppSettings
{
    public class ServerSettings
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [Url]
        public string Url { get; set; }
    }
}