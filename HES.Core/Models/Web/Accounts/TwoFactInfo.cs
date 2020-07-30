namespace HES.Core.Models.Web.Accounts
{
    public class TwoFactInformation
    {
        public bool Is2faEnabled { get; set; }
        public bool HasAuthenticator { get; set; }
        public int RecoveryCodesLeft { get; set; }
        public bool IsMachineRemembered { get; set; }
    }
}