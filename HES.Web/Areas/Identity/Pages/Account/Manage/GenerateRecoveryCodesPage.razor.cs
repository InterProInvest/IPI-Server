using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class GenerateRecoveryCodesPage : ComponentBase
    {
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<GenerateRecoveryCodesPage> Logger { get; set; }

        public ApplicationUser AppUser { get; set; }

        public string[] RecoveryCodes { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                AppUser = await UserManager.GetUserAsync(state.User);

                var isTwoFactorEnabled = await UserManager.GetTwoFactorEnabledAsync(AppUser);
                if (!isTwoFactorEnabled)
                {
                    Logger.LogError($"Cannot generate recovery codes for user with ID '{AppUser.Id}' because they do not have 2FA enabled.");
                    ToastService.ShowToast($"Cannot generate recovery codes for user with ID '{AppUser.Id}' because they do not have 2FA enabled.", ToastLevel.Error);
                    NavigationManager.NavigateTo("/Identity/Account/Manage/TwoFactorAuthentication", false);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }

        private async Task GenerateRecoveryCodesAsync()
        {
            try
            {
                var recoveryCodes = await UserManager.GenerateNewTwoFactorRecoveryCodesAsync(AppUser, 10);
                RecoveryCodes = recoveryCodes.ToArray();

                Logger.LogInformation($"User with ID '{AppUser.Id}' has generated new 2FA recovery codes.");
                ToastService.ShowToast("You have generated new recovery codes.", ToastLevel.Success);
                NavigationManager.NavigateTo($"/Identity/Account/Manage/ShowRecoveryCodes/{string.Join("&", RecoveryCodes)}", false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }
    }
}