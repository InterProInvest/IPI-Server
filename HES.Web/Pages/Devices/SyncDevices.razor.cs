using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Devices
{
    public partial class SyncDevices : ComponentBase
    {
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }
        [Inject] IDeviceService DeviceService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] ILogger<SyncDevices> Logger { get; set; }

        private async Task Sync()
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("showSpinner", "syncBtnSpinner");
                await DeviceService.ImportDevicesAsync();
                await JSRuntime.InvokeVoidAsync("showSpinner", "syncBtnSpinner");
                NavigationManager.NavigateTo("/Devices", true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast("Somethings went wrong.", ToastLevel.Error);
                await JSRuntime.InvokeVoidAsync("showSpinner", "syncBtnSpinner");
            }
        }
    }
}