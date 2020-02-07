using HES.Core.Interfaces;
using HES.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Logs
{
    public partial class LogsViewer : ComponentBase
    {
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] ILogsViewerService LogsViewerService { get; set; }
        public List<string> LogsFiles { get; set; }
        public List<LogModel> Logs { get; set; }

        private bool isBusy;


        protected override void OnInitialized()
        {
            LogsFiles = LogsViewerService.GetFiles();
        }

        private async Task ShowLog(string name)
        {
            if (isBusy)
            {
                return;
            }


            try
            {
                isBusy = true;

                if (Logs != null)
                {
                    await JSRuntime.InvokeVoidAsync("destroyLogsTable");
                    //StateHasChanged();
                }
                Logs = await LogsViewerService.GetLogAsync(name);
                //StateHasChanged();

                await JSRuntime.InvokeVoidAsync("initializeLogsTable");
            }
            catch (Exception)
            {
            }
            finally
            {
                isBusy = false;
            }
        }
    }
}
