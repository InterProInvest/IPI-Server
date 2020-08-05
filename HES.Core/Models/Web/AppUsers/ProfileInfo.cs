using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.AppUsers
{
    public class ProfileInfo
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
    }
}