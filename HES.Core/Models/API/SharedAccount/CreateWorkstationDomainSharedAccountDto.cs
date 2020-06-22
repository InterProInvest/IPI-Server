using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API.SharedAccount
{
    public class CreateWorkstationDomainSharedAccountDto : CreateWorkstationSharedAccountDto
    {
        [Required]
        public string Domain { get; set; }
    }
}