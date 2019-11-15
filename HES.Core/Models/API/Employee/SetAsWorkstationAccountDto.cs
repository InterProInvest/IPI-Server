using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class SetAsWorkstationAccountDto
    {
        [Required]
        public string DeviceId { get; set; }
        [Required]
        public string DeviceAccountId { get; set; }
    }
}