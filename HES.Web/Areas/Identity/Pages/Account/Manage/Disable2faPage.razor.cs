using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class Disable2faPage : ComponentBase
    {
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ILogger<Disable2faPage> Logger { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        public ApplicationUser AppUser { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            AppUser = await UserManager.GetUserAsync(state.User);

            var isTwoFactorEnabled = await UserManager.GetTwoFactorEnabledAsync(AppUser);
            if (!isTwoFactorEnabled)
            {
                Logger.LogError($"Cannot disable 2FA for user with ID '{AppUser.Id}' as it's not currently enabled.");
                ToastService.ShowToast($"Cannot disable 2FA for user with ID '{AppUser.Id}' as it's not currently enabled.", ToastLevel.Error);
                NavigationManager.NavigateTo("/Identity/Account/Manage/TwoFactorAuthentication", true);
            }
        }

        private async Task DisableTwoFactorAsync()
        {
            var disable2faResult = await UserManager.SetTwoFactorEnabledAsync(AppUser, false);
            if (!disable2faResult.Succeeded)
            {
                Logger.LogError($"Unexpected error occurred disabling 2FA for user with ID '{AppUser.Id}'.");
                ToastService.ShowToast($"Unexpected error occurred disabling 2FA for user with ID '{AppUser.Id}'.", ToastLevel.Error);
                NavigationManager.NavigateTo("/Identity/Account/Manage/TwoFactorAuthentication", true);
            }

            Logger.LogInformation($"User with ID '{AppUser.Id}' has disabled 2fa.");
            ToastService.ShowToast("2fa has been disabled. You can reenable 2fa when you setup an authenticator app", ToastLevel.Success);
            NavigationManager.NavigateTo("/Identity/Account/Manage/TwoFactorAuthentication", true);
        }
    }
}
