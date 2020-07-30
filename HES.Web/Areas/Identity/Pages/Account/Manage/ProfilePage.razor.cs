using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class ProfilePage : ComponentBase
    {
        [Inject] public ILogger<ProfilePage> Logger { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IEmailSenderService EmailSender { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        public ProfileInput ProfileInputModel { get; set; }
        public PasswordInput PasswordInputModel { get; set; } = new PasswordInput();

        protected override async Task OnInitializedAsync()
        {
            var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = await UserManager.GetUserAsync(state.User);

            ProfileInputModel = new ProfileInput
            {
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsEmailConfirmed = await UserManager.IsEmailConfirmedAsync(user)
            };
        }


        private async Task SaveProfileAsync()
        {
            var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = await UserManager.GetUserAsync(state.User);

            var email = await UserManager.GetEmailAsync(user);
            if (ProfileInputModel.Email != email)
            {
                var setEmailResult = await UserManager.SetEmailAsync(user, ProfileInputModel.Email);
                if (!setEmailResult.Succeeded)
                {
                    ToastService.ShowToast($"Unexpected error occurred setting email for user with ID '{user.Id}'.", ToastLevel.Error);
                    return;
                }
            }

            var phoneNumber = await UserManager.GetPhoneNumberAsync(user);
            if (ProfileInputModel.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await UserManager.SetPhoneNumberAsync(user, ProfileInputModel.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    ToastService.ShowToast($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.", ToastLevel.Error);
                    return;
                }
            }

            ToastService.ShowToast("Your profile has been updated.", ToastLevel.Success);
        }

        private async Task SendVerificationEmailAsync()
        {
            var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = await UserManager.GetUserAsync(state.User);


            var userId = user.Id;
            var email = await UserManager.GetEmailAsync(user);
            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);

            var callbackUrl = $"{NavigationManager.BaseUri}Identity/Account/ConfirmEmail?userId={userId}&code={WebUtility.UrlEncode(code)}";

            await EmailSender.SendUserConfirmEmailAsync(email, HtmlEncoder.Default.Encode(callbackUrl));
            ToastService.ShowToast("Verification email sent. Please check your email.", ToastLevel.Error);
        }

        public async Task ChangePasswordAsync()
        {
            var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = await UserManager.GetUserAsync(state.User);

            var changePasswordResult = await UserManager.ChangePasswordAsync(user, PasswordInputModel.OldPassword, PasswordInputModel.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                var errorMessage = string.Empty;
                foreach (var error in changePasswordResult.Errors)
                    errorMessage += error.Description + " ";
                
                ToastService.ShowToast(errorMessage, ToastLevel.Error);
                return;
            }

            Logger.LogInformation("User changed their password successfully.");
            ToastService.ShowToast("Your password has been changed.", ToastLevel.Success);
        }
    }

    public class ProfileInput
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        public bool IsEmailConfirmed { get; set; }
    }

    public class PasswordInput
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
