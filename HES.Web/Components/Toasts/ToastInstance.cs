using System;
using HES.Core.Enums;
using Microsoft.AspNetCore.Components;

namespace HES.Web.Components.Toasts
{
    public class ToastInstance
    {
        public string Id { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public string ToastTypeClass { get; private set; }

        public string Header { get; set; }
        public RenderFragment Message { get; set; }

        public ToastInstance(ToastType toastType, RenderFragment message, string header = "")
        {
            Id = Guid.NewGuid().ToString();
            TimeStamp = DateTime.Now;
            Message = message;

            switch (toastType)
            {
                case ToastType.Success:
                    ToastTypeClass = "alert-success";
                    Header = string.IsNullOrWhiteSpace(header) ? "Success" : header;
                    break;
                case ToastType.Notify:
                    ToastTypeClass = "alert-secondary";
                    Header = string.IsNullOrWhiteSpace(header) ? "Notification" : header;
                    break;
                case ToastType.Error:
                    ToastTypeClass = "alert-danger";
                    Header = string.IsNullOrWhiteSpace(header) ? "Error" : header;
                    break;
                default:
                    break;
            }
        }
    }
}
