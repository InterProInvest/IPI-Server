using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Devices
{
    public partial class SetProfile : ComponentBase
    {
        [Inject]
        public IHardwareVaultService HardwareVaultService { get; set; }

        [Inject]
        public IModalDialogService ModalDialogService { get; set; }

        [Inject]
        public ILogger<SetProfile> Logger { get; set; }

        [Inject]
        IToastService ToastService { get; set; }

        [Parameter]
        public EventCallback Refresh { get; set; }

        [Parameter]
        public string HardwareVaultId { get; set; }

        public SelectList VaultProfiles { get; set; }
        public string SelectedVaultProfileId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            VaultProfiles = new SelectList(await HardwareVaultService.GetProfilesAsync(), nameof(HardwareVaultProfile.Id), nameof(HardwareVaultProfile.Name));
            SelectedVaultProfileId = VaultProfiles.First().Value;
        }

        private async Task CloseAsync()
        {
            await ModalDialogService.CloseAsync();
        }

        private async Task SetVaultProfileAsync()
        {
            try
            {
                await HardwareVaultService.ChangeVaultProfileAsync(HardwareVaultId, SelectedVaultProfileId);
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Success vault profile updated", ToastLevel.Success);
                await CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await CloseAsync();
            }
        }
    }
}
