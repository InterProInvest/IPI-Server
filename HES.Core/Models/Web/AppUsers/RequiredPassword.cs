using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.AppUsers
{
    public class RequiredPassword
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
