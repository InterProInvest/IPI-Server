using System;

namespace HES.Core.Models.Web.SharedAccounts
{
    public class SharedAccountsFilter
    {
        public string Name { get; set; }
        public string Urls { get; set; }
        public string Apps { get; set; }
        public string Login { get; set; }
        public DateTime? PasswordUpdatedStart { get; set; }
        public DateTime? PasswordUpdatedEnd { get; set; }
        public DateTime? OtpUpdatedStart { get; set; }
        public DateTime? OtpUpdatedEnd { get; set; }
    }
}
