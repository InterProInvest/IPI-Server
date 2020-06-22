using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.SharedAccounts
{
    public partial class EditSharedAccount : ComponentBase
    {
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public ISharedAccountService SharedAccountService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EditSharedAccountOtp> Logger { get; set; }

        [Parameter] public SharedAccount Account { get; set; }

        protected override void OnInitialized()
        {
            Account.ConfirmPassword = Account.Password;
            ModalDialogService.OnCancel += CancelAsync;
        }

        private async Task EditAccountAsync()
        {
            try
            {
                var vaults = await SharedAccountService.EditSharedAccountAsync(Account);
                RemoteWorkstationConnectionsService.StartUpdateRemoteDevice(vaults);
                ToastService.ShowToast("Shared account updated.", ToastLevel.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await CancelAsync();
            }
        }

        private async Task CancelAsync()
        {
            await SharedAccountService.UnchangedAsync(Account);
            ModalDialogService.OnCancel -= CancelAsync;
        }
    }
}
