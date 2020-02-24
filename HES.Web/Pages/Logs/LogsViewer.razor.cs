using HES.Core.Interfaces;
using HES.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Logs
{
    public partial class LogsViewer : ComponentBase
    {
        [Inject] ILogsViewerService LogsViewerService { get; set; }
        [Inject] ILogger<LogsViewer> Logger { get; set; }

        public List<string> LogsFiles { get; set; }
        public List<LogModel> Logs { get; set; }
        public LogModel LogModel { get; set; }

        private bool isBusy;

        protected override void OnInitialized()
        {
            LogsFiles = LogsViewerService.GetFiles();
        }

        private async Task ShowLogAsync(string name)
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
                }

                Logs = await LogsViewerService.GetLogAsync(name);
                StateHasChanged();

                await JSRuntime.InvokeVoidAsync("initializeLogsTable");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
            finally
            {
                isBusy = false;
            }
        }

        private void ShowDetails(LogModel model)
        {
            LogModel = model;
        }       
    }
}