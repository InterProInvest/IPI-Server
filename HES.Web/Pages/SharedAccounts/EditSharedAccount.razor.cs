using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.SharedAccounts
{
    public partial class EditSharedAccount : ComponentBase, IDisposable
    {
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public ISharedAccountService SharedAccountService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<EditSharedAccount> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public string AccountId { get; set; }

        public SharedAccount Account { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public bool EntityBeingEdited { get; set; }
        public bool Initialized { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ModalDialogService.OnCancel += OnCancelAsync;

                Account = await SharedAccountService.GetSharedAccountByIdAsync(AccountId);
                if (Account == null)
                    throw new Exception("Account not found");

                EntityBeingEdited = MemoryCache.TryGetValue(Account.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Account.Id, Account);

                Account.ConfirmPassword = Account.Password;

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task EditAccountAsync()
        {
            try
            {
                var vaults = await SharedAccountService.EditSharedAccountAsync(Account);
                RemoteWorkstationConnectionsService.StartUpdateRemoteDevice(vaults);
                ToastService.ShowToast("Shared account updated.", ToastLevel.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.SharedAccounts);
                await ModalDialogService.CloseAsync();
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(SharedAccount.Name), ex.Message);
            }
            catch (IncorrectUrlException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(SharedAccount.Urls), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task OnCancelAsync()
        {
            await SharedAccountService.UnchangedAsync(Account);
        }

        public void Dispose()
        {
            ModalDialogService.OnCancel -= OnCancelAsync;

            if (!EntityBeingEdited)
                MemoryCache.Remove(Account.Id);
        }
    }
}
