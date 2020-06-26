namespace HES.Core.Models.Web.Users
{
    public class ApplicationUserFilter
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactorEnabled { get; set; }
    }
}
