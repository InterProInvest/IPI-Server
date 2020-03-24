using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.AppSettings
{
    public class Server
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [Url]
        public string Url { get; set; }
    }
}