﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Accounts;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.SharedAccounts
{
    public partial class EditSharedAccountOtp : ComponentBase, IDisposable
    {
        [Inject] public ISharedAccountService SharedAccountService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public ILogger<EditSharedAccountOtp> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public SharedAccount Account { get; set; }

        public AccountOtp AccountOtp { get; set; } = new AccountOtp();
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override void OnInitialized()
        {
            ModalDialogService.OnCancel += OnCancelAsync;

            EntityBeingEdited = MemoryCache.TryGetValue(Account.Id, out object _);
            if (!EntityBeingEdited)
                MemoryCache.Set(Account.Id, Account);
        }

        private async Task OnCancelAsync()
        {
            await SharedAccountService.UnchangedAsync(Account);
        }

        private async Task EditAccoountOtpAsync()
        {
            try
            {
                var vaults = await SharedAccountService.EditSharedAccountOtpAsync(Account, AccountOtp);
                RemoteWorkstationConnectionsService.StartUpdateRemoteDevice(vaults);
                ToastService.ShowToast("Account otp updated.", ToastLevel.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.SharedAccounts);
                await ModalDialogService.CloseAsync();
            }
            catch (IncorrectOtpException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(SharedAccount.OtpSecret), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        public void Dispose()
        {
            ModalDialogService.OnCancel -= OnCancelAsync;
            if (!EntityBeingEdited)
                MemoryCache.Remove(Account.Id);
        }
    }
}