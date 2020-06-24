using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.HardwareVaultAccessProfile
{
    public partial class CreateAccessProfile : ComponentBase
    {
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<CreateAccessProfile> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Inject] public IHardwareVaultService HardwareVaultService { get; set; }

        [Parameter] public string ConnectionId { get; set; }

        public HardwareVaultProfile AccessProfile { get; set; }

        protected override void OnInitialized()
        {
            AccessProfile = new HardwareVaultProfile
            {
                ButtonBonding = true,
                MasterKeyBonding = true,
                PinExpirationConverted = 1,
                PinLength = 4,
                PinTryCount = 3
            };
        }

        private async Task CreateProfileAsync()
        {
            try
            {
                await HardwareVaultService.CreateProfileAsync(AccessProfile);
                ToastService.ShowToast("Access profile created.", ToastLevel.Success);
                await HubContext.Clients.All.SendAsync(RefreshPage.HardwareVaultProfiles, ConnectionId);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private void OnInputPinExpiration(ChangeEventArgs args)
        {
            AccessProfile.PinExpirationConverted = Convert.ToInt32((string)args.Value); 
        }

        private void OnInputPinLength(ChangeEventArgs args)
        {
            AccessProfile.PinLength = Convert.ToInt32((string)args.Value);
        }

        private void OnInputPinTryCount(ChangeEventArgs args)
        {
            AccessProfile.PinTryCount = Convert.ToInt32((string)args.Value);
        }
    }
}
