using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Devices
{
    public partial class EditVaultRFID : ComponentBase
    {
        [Inject] 
        public IDeviceService HardwareVaultService { get; set; }

        [Inject]
        public ILogger<EditVaultRFID> Logger { get; set; }
        
        [Inject] 
        public IModalDialogService ModalDialogService { get; set; }
        
        [Inject] 
        IToastService ToastService { get; set; }
        
        [Parameter] 
        public EventCallback Refresh { get; set; }
        
        [Parameter] 
        public string HardwareVaultId { get; set; }

        public Device HardwareVault { get; set; }

        public ValidationErrorMessage ValidationErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                HardwareVault = await HardwareVaultService.GetDeviceByIdAsync(HardwareVaultId);

                if (HardwareVault == null)
                {
                    throw new Exception("Vault not found");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        private async Task EditAsync()
        {
            try
            {
                await HardwareVaultService.EditRfidAsync(HardwareVault);
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("RFID updated.", ToastLevel.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        private async Task CloseAsync()
        {
            await HardwareVaultService.UnchangedVaultAsync(HardwareVault);
            await ModalDialogService.CloseAsync();
        }
    }
}
