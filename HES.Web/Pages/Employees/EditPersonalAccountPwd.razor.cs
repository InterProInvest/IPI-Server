using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Accounts;
using HES.Core.Models.Web.AppSettings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Web.Pages.Employees
{
    public partial class EditPersonalAccountPwd : ComponentBase, IDisposable
    {
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILdapService LdapService { get; set; }
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EditPersonalAccountPwd> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public Account Account { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public LdapSettings LdapSettings { get; set; }
        public bool EntityBeingEdited { get; set; }

        private AccountPassword _accountPassword = new AccountPassword();
        private bool _isBusy;
        private bool _initialized;

        protected override async Task OnInitializedAsync()
        {
            ModalDialogService.OnCancel += ModalDialogService_OnCancel;
            EntityBeingEdited = MemoryCache.TryGetValue(Account.Id, out object _);
            if (!EntityBeingEdited)
                MemoryCache.Set(Account.Id, Account);

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
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.EmployeesDetails, Account.EmployeeId, Account.Id);
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

        private async Task ModalDialogService_OnCancel()
        {
            await EmployeeService.UnchangedPersonalAccountAsync(Account);
        }

        public void Dispose()
        {
            ModalDialogService.OnCancel -= ModalDialogService_OnCancel;

            if (!EntityBeingEdited)
                MemoryCache.Remove(Account.Id);
        }
    }
}