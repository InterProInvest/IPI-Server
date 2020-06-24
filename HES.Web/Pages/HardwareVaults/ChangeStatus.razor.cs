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
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Parameter] public string HardwareVaultId { get; set; }
        [Parameter] public VaultStatus VaultStatus { get; set; }

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
                        ToastService.ShowToast("Vault pending activate.", ToastLevel.Success);
                        break;
                    case VaultStatus.Suspended:
                        await HardwareVaultService.SuspendVaultAsync(HardwareVaultId, StatusDescription);
                        ToastService.ShowToast("Vault pending suspend.", ToastLevel.Success);
                        break;
                    case VaultStatus.Compromised:
                        if (CompromisedIsDisabled)
                            return;
                        await HardwareVaultService.VaultCompromisedAsync(HardwareVaultId, StatusReason, StatusDescription);
                        ToastService.ShowToast("Vault compromised.", ToastLevel.Success);
                        break;
                }

                RemoteWorkstationConnectionsService.StartUpdateRemoteDevice(HardwareVaultId);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
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