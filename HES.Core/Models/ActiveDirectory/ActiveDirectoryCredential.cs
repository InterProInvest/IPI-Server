using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.ActiveDirectory
{
    public class ActiveDirectoryCredential
    {
        [Required]
        [Display(Name = "Domain")]
        public string Host { get; set; }

        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}