using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class ShowRecoveryCodesPage : ComponentBase
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Parameter] public string Codes { get; set; }
        public string[] RecoveryCodes { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await BreadcrumbsService.SetShowRecoveryCodes();
            RecoveryCodes = Codes.Split("&");
            if (RecoveryCodes == null || RecoveryCodes.Length == 0)
            {
                NavigationManager.NavigateTo("/Identity/Account/Manage/TwoFactorAuthentication");
            }
        }
    }
}