using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.SharedAccounts
{
    public partial class EditSharedAccountPassword : ComponentBase
    {
        [Inject] public ISharedAccountService SharedAccountService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public ILogger<EditSharedAccountOtp> Logger { get; set; }
        [Inject] public IHubContext<SharedAccountsHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public SharedAccount Account { get; set; }

        public AccountPassword AccountPassword { get; set; } = new AccountPassword();

        private async Task EditAccoountPasswordAsync()
        {
            try
            {
                var vaults = await SharedAccountService.EditSharedAccountPwdAsync(Account, AccountPassword);
                RemoteWorkstationConnectionsService.StartUpdateRemoteDevice(vaults);
                ToastService.ShowToast("Account password updated.", ToastLevel.Success);
                await HubContext.Clients.All.SendAsync("PageUpdated", ConnectionId);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }
    }
}