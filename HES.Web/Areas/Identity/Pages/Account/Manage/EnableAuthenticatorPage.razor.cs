using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class EnableAuthenticatorPage : ComponentBase
    {
        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public UrlEncoder UrlEncoder { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EnableAuthenticatorPage> Logger { get; set; }

        public ApplicationUser AppUser { get; set; }
        public string SharedKey { get; set; } = string.Empty;
        public string AuthenticatorUri { get; set; } = string.Empty;
        public string[] RecoveryCodes { get; set; }
        public InputCode InputCode { get; set; } = new InputCode();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                AppUser = await UserManager.GetUserAsync(state.User);

                await LoadSharedKeyAndQrCodeUriAsync(AppUser);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("generateQr", AuthenticatorUri);
            }
        }

        private async Task VerifyTwoFactorAsync()
        {
            try
            {
                var verificationCode = InputCode.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

                var is2faTokenValid = await UserManager.VerifyTwoFactorTokenAsync(
                    AppUser, UserManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

                if (!is2faTokenValid)
                {
                    ToastService.ShowToast("Verification code is invalid.", ToastLevel.Error);
                    await LoadSharedKeyAndQrCodeUriAsync(AppUser);
                    return;
                }

                await UserManager.SetTwoFactorEnabledAsync(AppUser, true);

                Logger.LogInformation($"User with ID '{AppUser.Id}' has enabled 2FA with an authenticator app.");
                ToastService.ShowToast("Your authenticator app has been verified.", ToastLevel.Success);


                if (await UserManager.CountRecoveryCodesAsync(AppUser) == 0)
                {
                    var recoveryCodes = await UserManager.GenerateNewTwoFactorRecoveryCodesAsync(AppUser, 10);
                    RecoveryCodes = recoveryCodes.ToArray();
                    NavigationManager.NavigateTo("/Identity/Account/Manage/ShowRecoveryCodes", false);
                }
                else
                {
                    NavigationManager.NavigateTo("/Identity/Account/Manage/TwoFactorAuthentication", false);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(ApplicationUser user)
        {
            var unformattedKey = await UserManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await UserManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await UserManager.GetAuthenticatorKeyAsync(user);
            }

            SharedKey = FormatKey(unformattedKey);

            var email = await UserManager.GetEmailAsync(user);
            AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey);
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                AuthenticatorUriFormat,
                UrlEncoder.Encode("HES.Web"),
                UrlEncoder.Encode(email),
                unformattedKey);
        }
    }

    public class InputCode
    {
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Verification Code")]
        public string Code { get; set; }
    }
}