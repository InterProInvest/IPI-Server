using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models;
using HES.Core.Models.Web.AppSettings;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Web.Pages.Employees
{
    public partial class EditPersonalAccountPwd : ComponentBase
    {
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILdapService LdapService { get; set; }
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EditPersonalAccountPwd> Logger { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }
        [Parameter] public Account Account { get; set; }

        public LdapSettings LdapSettings { get; set; }

        private AccountPassword _accountPassword = new AccountPassword();
        private bool _isBusy;
        private bool _initialized;

        protected override async Task OnInitializedAsync()
        {
            LdapSettings = await AppSettingsService.GetLdapSettingsAsync();
            _initialized = true;
        }

        private async Task EditAccountPasswordAsync()
        {
            try
            {
                if (_isBusy)
                    return;

                _isBusy = true;

                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await EmployeeService.EditPersonalAccountPwdAsync(Account, _accountPassword);

                    if (_accountPassword.UpdateActiveDirectoryPassword)
                        await LdapService.SetUserPasswordAsync(Account.EmployeeId, _accountPassword.Password, LdapSettings);

                    transactionScope.Complete();
                }

                RemoteWorkstationConnectionsService.StartUpdateRemoteDevice(await EmployeeService.GetEmployeeVaultIdsAsync(Account.EmployeeId));
                ToastService.ShowToast("Account password updated.", ToastLevel.Success);
                await Refresh.InvokeAsync(this);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
            }
            finally
            {
                _isBusy = false;
            }
        }
    }
}