using Microsoft.AspNetCore.Components;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class ShowRecoveryCodesPage : ComponentBase
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Parameter] public string Codes { get; set; }
        public string[] RecoveryCodes { get; set; }

        protected override void OnInitialized()
        {
            RecoveryCodes = Codes.Split("&");
            if (RecoveryCodes == null || RecoveryCodes.Length == 0)
            {
                NavigationManager.NavigateTo("/Identity/Account/Manage/TwoFactorAuthentication");
            }
        }
    }
}