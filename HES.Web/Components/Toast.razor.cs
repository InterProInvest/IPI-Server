using HES.Core.Enums;
using Microsoft.AspNetCore.Components;
using System;

namespace HES.Web.Components
{
    public partial class Toast : ComponentBase, IDisposable
    {
        protected string Header { get; set; }
        protected string Message { get; set; }
        protected string LevelClass { get; set; }
        protected bool IsVisible { get; set; }

        protected override void OnInitialized()
        {
            ToastService.OnShow += ToastService_OnShow;
            ToastService.OnHide += ToastService_OnHide;
        }

        private void ToastService_OnShow(string message, ToastLevel level)
        {
            InvokeAsync(() =>
            {
                BuildToastSettings(message, level);
                IsVisible = true;
                StateHasChanged();
            });
        }

        private void ToastService_OnHide()
        {
            InvokeAsync(() =>
            {
                IsVisible = false;
                StateHasChanged();
            });
        }

        private void BuildToastSettings(string message, ToastLevel level)
        {
            switch (level)
            {
                case ToastLevel.Success:
                    LevelClass = "alert-success custom-success-alert";
                    Header = "Success";
                    break;
                case ToastLevel.Error:
                    LevelClass = "alert-danger custom-error-alert";
                    Header = "Error";
                    break;
            }

            Message = message;
        }

        public void Dispose()
        {
            ToastService.OnShow -= ToastService_OnShow;
            ToastService.OnHide -= ToastService_OnHide;
        }
    }
}