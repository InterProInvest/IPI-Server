using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.SharedAccounts
{
    public partial class EditSharedAccountOtp : ComponentBase
    {
        [Inject] public ISharedAccountService SharedAccountService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EditSharedAccountOtp> Logger { get; set; }
        [Parameter] public SharedAccount Account { get; set; }

        private async Task EditAccoountOtpAsync()
        {
            try
            {
                var account = await SharedAccountService.EditSharedAccountOtpAsync(Account);
                ToastService.ShowToast("Account otp updated.", ToastLevel.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ModalDialogService.CloseAsync();
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }

        private async Task CloseAsync()
        {
            await ModalDialogService.CloseAsync();
            await SharedAccountService.UnchangedAsync(Account);
        }
    }
}
