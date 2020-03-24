using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class ModalDialogService : IModalDialogService
    {
        public event Func<string, RenderFragment, ModalDialogSize, Task> OnShow;
        public event Func<Task> OnClose;

        public async Task ShowAsync(string title, RenderFragment body)
        {
            await ShowAsync(title, body, ModalDialogSize.Default);
        }

        public async Task ShowAsync(string title, RenderFragment body, ModalDialogSize size)
        {
            await OnShow?.Invoke(title, body, size);
        }

        public async Task CloseAsync()
        {
            await OnClose?.Invoke();
        }
    }
}