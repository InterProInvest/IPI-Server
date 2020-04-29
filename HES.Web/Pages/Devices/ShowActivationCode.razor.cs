using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace HES.Web.Pages.Devices
{
    public partial class ShowActivationCode : ComponentBase
    {
        [Inject]
        public IHardwareVaultService HardwareVaultService { get; set; }


        [Inject]
        public IModalDialogService ModalDialogService { get; set; }

        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        [Parameter]
        public HardwareVault HardwareVault { get; set; }

        public string ActivationCodeString { get; set; }
        public string InputType { get; private set; }

        protected override async Task OnInitializedAsync()
        {
            ActivationCodeString = await HardwareVaultService.GetVaultActivationCodeAsync(HardwareVault.Id);
            InputType = "Password";
        }

        private async Task SendOnEmailAsync()
        {
            
        }

        private async Task CopyToClipboardAsync()
        {
            await JsRuntime.InvokeVoidAsync("copyToClipboard");
        }

        private async Task CloseAsync()
        {
            ActivationCodeString = string.Empty;
            await ModalDialogService.CloseAsync();
        }
    }
}
