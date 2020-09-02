using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Parameters
{
    public partial class Parameters : OwningComponentBase, IDisposable
    {
        public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<Parameters> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        public string ApiAddress { get; set; }
        public string DomainHost { get; set; }
        public bool Initialized { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            AppSettingsService = ScopedServices.GetRequiredService<IAppSettingsService>();

            await InitializeHubAsync();
            await BreadcrumbsService.SetParameters();
            await LoadDataSettingsAsync();
            Initialized = true;
        }

        private async Task LoadDataSettingsAsync()
        {
            ApiAddress = await LoadLicensingSettingsAsync();
            DomainHost = await LoadDomainSettingsAsync();
        }

        private async Task<string> LoadLicensingSettingsAsync()
        {
            var licensingSettings = await AppSettingsService.GetLicensingSettingsAsync();
            return licensingSettings?.ApiAddress;
        }

        private async Task OpenDialogLicensingSettingsAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(LicenseSettingsDialog));
                builder.AddAttribute(1, nameof(LicenseSettingsDialog.ApiAddress), ApiAddress);
                builder.AddAttribute(2, nameof(LicenseSettingsDialog.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("License Settings", body);
        }

        private async Task<string> LoadDomainSettingsAsync()
        {
            var domainSettings = await AppSettingsService.GetLdapSettingsAsync();
            return domainSettings?.Host;
        }

        private async Task OpenDialogLdapSettingsAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(LdapSettingsDialog));
                builder.AddAttribute(1, nameof(LdapSettingsDialog.Host), DomainHost);
                builder.AddAttribute(2, nameof(LdapSettingsDialog.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Domain Settings", body);
        }

        private async Task OpenDialogDeleteLdapCredentialsAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteLdapCredentials));
                builder.AddAttribute(1, nameof(DeleteLdapCredentials.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete", body);
        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On<string>(RefreshPage.Parameters, async (connectionId) =>
            {
                await LoadDataSettingsAsync();
                StateHasChanged();
                if (hubConnection.ConnectionId != connectionId)
                    ToastService.ShowToast("Page updated by another admin.", ToastLevel.Notify);
            });

            await hubConnection.StartAsync();
        }

        public void Dispose()
        {
            _ = hubConnection?.DisposeAsync();
            AppSettingsService.Dispose();
        }
    }
}