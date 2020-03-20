using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API.Group
{
    public class CreateGroupDto
    {
        [Required]
        public string Name { get; set; }
        [RegularExpression(@"^[a-z0-9][-a-z0-9.!#$%&'*+-=?^_`{|}~\/]+@([-a-z0-9]+\.)+[a-z]{2,5}$", ErrorMessage = "The Email field is not a valid e-mail address.")]
        public string Email { get; set; }
        public string Description { get; set; }
    }
}