using HES.Core.Enums;
using System;

namespace HES.Core.Interfaces
{
    public interface IToastService
    {
        event Action<string, ToastLevel> OnShow;
        event Action OnHide;
        void ShowToast(string message, ToastLevel level);
    }
}