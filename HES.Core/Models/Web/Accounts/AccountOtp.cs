using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.Accounts
{
    public class AccountOtp
    {
        [Display(Name = "OTP secret")]
        public string OtpSecret { get; set; }
    }
}