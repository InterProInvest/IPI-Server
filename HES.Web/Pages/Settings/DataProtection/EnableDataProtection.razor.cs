using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.DataProtection
{
    public partial class EnableDataProtection : ComponentBase
    {
        public class NewPasswordModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        [Inject] public IDataProtectionService DataProtectionService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EnableDataProtection> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public NewPasswordModel NewPassword { get; set; } = new NewPasswordModel();
        public bool IsBusy { get; set; }

        private async Task EnableDataProtectionAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                await DataProtectionService.EnableProtectionAsync(NewPassword.Password);
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Data protection enabled.", ToastLevel.Success);
                Logger.LogInformation($"Data protection enabled by {authState.User.Identity.Name}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
            finally
            {
                IsBusy = false;

                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.DataProtection);
                await ModalDialogService.CloseAsync();
            }
        }
    }
}
