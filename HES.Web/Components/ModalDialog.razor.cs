using HES.Core.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public partial class ModalDialog : ComponentBase
    {
        public string ModalTitle { get; set; }
        public string ModalSize { get; set; }
        public RenderFragment ModalBody { get; set; }

        public async Task ShowAsync(string title, RenderFragment body, ModalDialogSize size = ModalDialogSize.Default)
        {
            SetModalSize(size);
            ModalTitle = title;
            ModalBody = body;

            await JSRuntime.InvokeVoidAsync("toggleModalDialog", "modalDialog");
            await InvokeAsync(StateHasChanged);
        }

        public async Task CloseAsync()
        {
            SetModalSize(ModalDialogSize.Default);
            ModalTitle = string.Empty;
            ModalBody = null;

            await JSRuntime.InvokeVoidAsync("toggleModalDialog", "modalDialog");
            await InvokeAsync(StateHasChanged);
        }

        private void SetModalSize(ModalDialogSize size)
        {
            switch (size)
            {
                case ModalDialogSize.Default:
                    ModalSize = string.Empty;
                    break;
                case ModalDialogSize.Small:
                    ModalSize = "modal-sm";
                    break;
                case ModalDialogSize.Large:
                    ModalSize = "modal-lg";
                    break;
                case ModalDialogSize.ExtraLarge:
                    ModalSize = "modal-xl";
                    break;
            }
        }
    }
}
