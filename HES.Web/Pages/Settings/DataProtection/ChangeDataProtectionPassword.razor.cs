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
    public partial class ChangeDataProtectionPassword : ComponentBase
    {
        public class CurrentPasswordModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string OldPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        [Inject] public IDataProtectionService DataProtectionService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<DisableDataProtection> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public CurrentPasswordModel CurrentPassword { get; set; } = new CurrentPasswordModel();
        public bool IsBusy { get; set; }

        private async Task ChangeDataProtectionPasswordAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {

                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                await DataProtectionService.ChangeProtectionPasswordAsync(CurrentPassword.OldPassword, CurrentPassword.NewPassword);
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Data protection password updated.", ToastLevel.Success);
                Logger.LogInformation($"Data protection password updated by {authState.User.Identity.Name}");
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