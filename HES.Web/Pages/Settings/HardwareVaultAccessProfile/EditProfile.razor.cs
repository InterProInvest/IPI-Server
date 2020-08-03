using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.HardwareVaultAccessProfile
{
    public partial class EditProfile : ComponentBase, IDisposable
    {
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<EditProfile> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Inject] public IHardwareVaultService HardwareVaultService { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public HardwareVaultProfile AccessProfile { get; set; }

        public bool EntityBeingEdited { get; set; }
        public int InitPinExpirationValue { get; set; }
        public int InitPinLengthValue { get; set; }
        public int InitPinTryCountValue { get; set; }


        protected override void OnInitialized()
        {
            ModalDialogService.OnCancel += OnCancelAsync;

            InitPinExpirationValue = AccessProfile.PinExpirationConverted;
            InitPinLengthValue = AccessProfile.PinLength;
            InitPinTryCountValue = AccessProfile.PinTryCount;

            EntityBeingEdited = MemoryCache.TryGetValue(AccessProfile.Id, out object _);
            if (!EntityBeingEdited)
                MemoryCache.Set(AccessProfile.Id, AccessProfile);
        }

        private async Task EditProfileAsync()
        {
            try
            {
                await HardwareVaultService.EditProfileAsync(AccessProfile);
                ToastService.ShowToast("Hardware vault profile updated.", ToastLevel.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.HardwareVaultProfiles);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task OnCancelAsync()
        {
            await HardwareVaultService.UnchangedProfileAsync(AccessProfile);
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

        public void Dispose()
        {
            ModalDialogService.OnCancel -= OnCancelAsync;

            if (!EntityBeingEdited)
                MemoryCache.Remove(AccessProfile.Id);
        }
    }
}