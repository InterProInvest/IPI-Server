using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.HardwareVaults
{
    public partial class ChangeProfile : ComponentBase, IDisposable
    {
        [Inject] public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public ILogger<ChangeProfile> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Parameter] public string HardwareVaultId { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public HardwareVault HardwareVault { get; set; }
        public SelectList VaultProfiles { get; set; }
        public string SelectedVaultProfileId { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                HardwareVault = await HardwareVaultService.GetVaultByIdAsync(HardwareVaultId);
                if (HardwareVault == null)
                    throw new Exception("HardwareVault not found.");

                EntityBeingEdited = MemoryCache.TryGetValue(HardwareVault.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(HardwareVault.Id, HardwareVault);

                VaultProfiles = new SelectList(await HardwareVaultService.GetProfilesAsync(), nameof(HardwareVaultProfile.Id), nameof(HardwareVaultProfile.Name));
                SelectedVaultProfileId = VaultProfiles.First().Value;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }     
        }

        private async Task CloseAsync()
        {
            await ModalDialogService.CloseAsync();
        }

        private async Task ChangeProfileAsync()
        {
            try
            {
                await HardwareVaultService.ChangeVaultProfileAsync(HardwareVault.Id, SelectedVaultProfileId);
                ToastService.ShowToast("Vault profile updated", ToastLevel.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.HardwareVaults);
                await CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(HardwareVault.Id);
        }
    }
}