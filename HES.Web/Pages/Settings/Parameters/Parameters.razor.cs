using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using HES.Core.Models.Web.Breadcrumb;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Parameters
{
    public partial class Parameters : ComponentBase
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; }
        [Inject]
        private IAppSettingsService AppSettingsService { get; set; }
        [Inject]
        private ILogger<Parameters> Logger { get; set; }

        private Licensing licensing = new Licensing();
        private Server server = new Server();
        private bool licensingIsBusy;
        private bool serverIsBusy;

        protected override async Task OnInitializedAsync()
        {
            await LoadLicensingSettingsAsync();
            await LoadServerSettingsAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await CreateBreadcrumbs();
            }
        }

        private async Task CreateBreadcrumbs()
        {
            var list = new List<Breadcrumb>() {
                new Breadcrumb () { Active = "active", Content = "Settings" },
                new Breadcrumb () { Active = "active", Content = "Parameters" }
            };

            await JSRuntime.InvokeVoidAsync("createBreadcrumbs", list);
        }

        private async Task LoadLicensingSettingsAsync()
        {
            licensing = await AppSettingsService.GetLicensingSettingsAsync();
        }

        private async Task UpdateLicensingSettingsAsync()
        {
            try
            {
                if (licensingIsBusy)
                {
                    return;
                }

                licensingIsBusy = true;
                await AppSettingsService.SetLicensingSettingsAsync(licensing);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
            finally
            {
                licensingIsBusy = false;
            }
        }

        private async Task LoadServerSettingsAsync()
        {
            server = await AppSettingsService.GetServerSettingsAsync();
        }

        private async Task UpdateServerSettingsAsync()
        {
            try
            {
                if (serverIsBusy)
                {
                    return;
                }

                serverIsBusy = true;
                await AppSettingsService.SetServerSettingsAsync(server);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
            finally
            {
                serverIsBusy = false;
            }
        }
    }
}