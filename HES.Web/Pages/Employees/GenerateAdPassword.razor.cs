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
    public partial class GenerateAdPassword : ComponentBase, IDisposable
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
        private bool Initialized { get; set; }

        protected override async Task OnInitializedAsync()
        {
            EntityBeingEdited = MemoryCache.TryGetValue(Account.Id, out object _);
            if (!EntityBeingEdited)
                MemoryCache.Set(Account.Id, Account);

            LdapSettings = await AppSettingsService.GetLdapSettingsAsync();
            Initialized = true;
        }

        private async Task GenerateAccountPasswordAsync()
        {
            try
            {
                if (LdapSettings?.Password == null)
                    throw new Exception("Active Directory credentials not set in parameters page.");

                var accountPassword = new AccountPassword() { Password = Guid.NewGuid().ToString() }; // TODO generate from Communication.dll

                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await LdapService.SetUserPasswordAsync(Account.EmployeeId, accountPassword.Password, LdapSettings);
                    await EmployeeService.EditPersonalAccountPwdAsync(Account, accountPassword);
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
                await ModalDialogService.CancelAsync();
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Account.Id);
        }
    }
}