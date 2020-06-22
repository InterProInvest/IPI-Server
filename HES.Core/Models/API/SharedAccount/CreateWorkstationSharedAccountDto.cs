using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API.SharedAccount
{
    public class CreateWorkstationSharedAccountDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}