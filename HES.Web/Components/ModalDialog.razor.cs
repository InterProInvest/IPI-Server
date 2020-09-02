using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public partial class ModalDialog : ComponentBase, IDisposable
    {
        [Inject] IModalDialogService ModalDialogService { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }
        public string ModalTitle { get; set; }
        public string ModalSize { get; set; }
        public RenderFragment ModalBody { get; set; }

        protected override void OnInitialized()
        {
            ModalDialogService.OnShow += ShowAsync;
            ModalDialogService.OnClose += CloseAsync;
            ModalDialogService.OnCancel += CancelAsync;
        }

        public async Task ShowAsync(string title, RenderFragment body, ModalDialogSize size)
        {
            SetModalSize(size);
            ModalTitle = title;
            ModalBody = body;

            await JSRuntime.InvokeVoidAsync("toggleModalDialog", "genericModalDialog");
            await InvokeAsync(StateHasChanged);
        }

        public async Task CloseAsync()
        {
            SetModalSize(ModalDialogSize.Default);
            ModalTitle = string.Empty;
            ModalBody = null;

            await JSRuntime.InvokeVoidAsync("toggleModalDialog", "genericModalDialog");
            await InvokeAsync(StateHasChanged);
        }

        public async Task CancelAsync()
        {
            await CloseAsync();
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ModalDialogService.OnShow -= ShowAsync;
                ModalDialogService.OnClose -= CloseAsync;
                ModalDialogService.OnCancel -= CancelAsync;
            }
        }
    }
}
