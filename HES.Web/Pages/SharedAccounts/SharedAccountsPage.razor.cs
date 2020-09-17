using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.SharedAccounts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.SharedAccounts
{
    public partial class SharedAccountsPage : OwningComponentBase, IDisposable
    {
        public ISharedAccountService SharedAccountService { get; set; }
        public IMainTableService<SharedAccount, SharedAccountsFilter> MainTableService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            SharedAccountService = ScopedServices.GetRequiredService<ISharedAccountService>();
            MainTableService = ScopedServices.GetRequiredService<IMainTableService<SharedAccount, SharedAccountsFilter>>();

            await InitializeHubAsync();
            await BreadcrumbsService.SetSharedAccounts();
            await MainTableService.InitializeAsync(SharedAccountService.GetSharedAccountsAsync, SharedAccountService.GetSharedAccountsCountAsync, ModalDialogService, StateHasChanged, nameof(SharedAccount.Name), ListSortDirection.Ascending);
        }

        private async Task CreateSharedAccountAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateSharedAccount));
                builder.AddAttribute(1, nameof(CreateSharedAccount.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Create Shared Account", body, ModalDialogSize.Large);
        }

        private async Task DeleteSharedAccountAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteSharedAccount));
                builder.AddAttribute(1, nameof(DeleteSharedAccount.AccountId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(DeleteSharedAccount.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Delete Shared Account", body, ModalDialogSize.Default);
        }

        private async Task EditSharedAccountOTPAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditSharedAccountOtp));
                builder.AddAttribute(1, nameof(EditSharedAccountOtp.AccountId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(EditSharedAccountOtp.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit Shared Account OTP", body, ModalDialogSize.Default);
        }

        private async Task EditSharedAccountAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditSharedAccount));
                builder.AddAttribute(1, nameof(EditSharedAccount.AccountId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(EditSharedAccount.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit Shared Account", body, ModalDialogSize.Default);
        }

        private async Task EditSharedAccountPasswordAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditSharedAccountPassword));
                builder.AddAttribute(1, nameof(EditSharedAccountPassword.AccountId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(EditSharedAccountPassword.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit Shared Account Password", body, ModalDialogSize.Default);
        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On(RefreshPage.SharedAccounts, async () =>
            {
                await MainTableService.LoadTableDataAsync();
                ToastService.ShowToast("Page updated by another admin.", ToastLevel.Notify);
            });

            await hubConnection.StartAsync();
        }

        public void Dispose()
        {
            if (hubConnection?.State == HubConnectionState.Connected)
                hubConnection.DisposeAsync();

            MainTableService.Dispose();
        }
    }
}