using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.AppUsers
{
    public class Invitation
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}