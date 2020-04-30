using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.HardwareVaults
{
    public partial class ChangeStatus : ComponentBase
    {
        [Inject] public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public ILogger<ChangeStatus> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Parameter] public string HardwareVaultId { get; set; }
        [Parameter] public VaultStatus VaultStatus { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public string StatusDescription { get; set; }
        public VaultStatusReason StatusReason { get; set; } = VaultStatusReason.Lost;
        public string CompromisedConfirmText { get; set; } = string.Empty;
        public bool CompromisedIsDisabled { get; set; } = true;

        private async Task ChangeStatusAsync()
        {
            try
            {
                switch (VaultStatus)
                {
                    case VaultStatus.Active:
                        await HardwareVaultService.ActivateVaultAsync(HardwareVaultId);
                        break;
                    case VaultStatus.Suspended:
                        await HardwareVaultService.SuspendVaultAsync(HardwareVaultId, StatusDescription);
                        break;
                    case VaultStatus.Compromised:
                        if (CompromisedIsDisabled)
                            return;
                        await HardwareVaultService.VaultCompromisedAsync(HardwareVaultId, StatusReason, StatusDescription);
                        break;
                }

                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Vault suspended", ToastLevel.Success);
                await CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await CloseAsync();
            }
        }

        private async Task CloseAsync()
        {
            await ModalDialogService.CloseAsync();
        }

        private void CompromisedConfirm()
        {
            if (CompromisedConfirmText == HardwareVaultId)
            {
                CompromisedIsDisabled = false;
            }
            else
            {
                CompromisedIsDisabled = true;
            }
        }
    }
}