using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class EditSharedAccountOtpDto
    {
        [Required]
        public string Id { get; set; }
        public string OtpSecret { get; set; }
    }
}