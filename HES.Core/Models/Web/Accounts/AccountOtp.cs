using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.Account
{
    public class AccountOtp
    {
        [Display(Name = "OTP secret")]
        public string OtpSecret { get; set; }
    }
}