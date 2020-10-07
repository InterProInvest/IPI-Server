﻿using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.DataProtection
{
    public partial class DisableDataProtection : ComponentBase
    {
        public class CurrentPasswordModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }
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
        public ButtonSpinner ButtonSpinner { get; set; }

        private async Task DisableDataProtectionAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                    await DataProtectionService.DisableProtectionAsync(CurrentPassword.Password);
                    await Refresh.InvokeAsync(this);
                    await ToastService.ShowToastAsync("Data protection disabled.", ToastType.Success);
                    Logger.LogInformation($"Data protection disabled by {authState.User.Identity.Name}");
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
            finally
            {  
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.DataProtection);
                await ModalDialogService.CloseAsync();
            }
        }
    }
}
