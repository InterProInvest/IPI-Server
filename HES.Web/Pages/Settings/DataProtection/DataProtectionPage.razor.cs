using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.DataProtection
{
    public partial class DataProtectionPage : ComponentBase, IDisposable
    {
        [Inject] public IDataProtectionService DataProtectionService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ILogger<DataProtectionPage> Logger { get; set; }

        public ProtectionStatus Status { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            ProtectionStatus();
            await InitializeHubAsync();
            await BreadcrumbsService.SetDataProtection();
        }

        private void ProtectionStatus()
        {
            Status = DataProtectionService.Status();
            StateHasChanged();
        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On(RefreshPage.DataProtection, () =>
            {
                ProtectionStatus();
                ToastService.ShowToast("Page updated by another admin.", ToastLevel.Notify);
            });

            await hubConnection.StartAsync();
        }

        private async Task EnableDataProtectionAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EnableDataProtection));
                builder.AddAttribute(1, nameof(EnableDataProtection.Refresh), EventCallback.Factory.Create(this, ProtectionStatus));
                builder.AddAttribute(2, nameof(EnableDataProtection.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Enable Data Protection", body, ModalDialogSize.Default);
        }

        private async Task ChangeDataProtectionPasswordAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ChangeDataProtectionPassword));
                builder.AddAttribute(1, nameof(ChangeDataProtectionPassword.Refresh), EventCallback.Factory.Create(this, ProtectionStatus));
                builder.AddAttribute(2, nameof(ChangeDataProtectionPassword.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Change Data Protection Password", body, ModalDialogSize.Default);
        }

        private async Task DisableDataProtectionAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DisableDataProtection));
                builder.AddAttribute(1, nameof(DisableDataProtection.Refresh), EventCallback.Factory.Create(this, ProtectionStatus));
                builder.AddAttribute(2, nameof(DisableDataProtection.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Disable Data Protection", body, ModalDialogSize.Default);
        }

        public void Dispose()
        {
            if (hubConnection?.State == HubConnectionState.Connected)
                hubConnection.DisposeAsync();
        }
    }
}
