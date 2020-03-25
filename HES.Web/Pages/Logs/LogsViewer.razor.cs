﻿using HES.Core.Interfaces;
using HES.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
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
        public string searchText = string.Empty;

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

            isBusy = true;

            try
            {
                Logs = await LogsViewerService.GetLogAsync(name);
                StateHasChanged();
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
    }
}