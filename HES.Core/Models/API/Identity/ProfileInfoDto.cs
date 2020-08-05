using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API.Identity
{
    public class ProfileInfoDto
    {
        [Required]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public ClaimsPrincipal ClaimsPrincipal { get; set; }
    }
}
