using HES.Core.Enums;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IModalDialogService
    {
        event Func<string, RenderFragment, ModalDialogSize, Task> OnShow;
        event Func<Task> OnClose;
        event Func<Task> OnCancel;

        Task ShowAsync(string title, RenderFragment body);
        Task ShowAsync(string title, RenderFragment body, ModalDialogSize size);
        Task CloseAsync();
        Task CancelAsync();
    }
}