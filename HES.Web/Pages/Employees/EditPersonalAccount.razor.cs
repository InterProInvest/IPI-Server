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

namespace HES.Web.Pages.Employees
{
    public partial class EditPersonalAccount : ComponentBase, IDisposable
    {
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EditPersonalAccount> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public Account Account { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public bool EntityBeingEdited { get; set; }

        private bool _isBusy;

        protected override void OnInitialized()
        {
            EntityBeingEdited = MemoryCache.TryGetValue(Account.Id, out object _);
            if (!EntityBeingEdited)
                MemoryCache.Set(Account.Id, Account);

            ModalDialogService.OnCancel += OnCancelAsync;
        }

        private async Task EditAccountAsync()
        {
            try
            {
                if (_isBusy)
                    return;

                _isBusy = true;

                await EmployeeService.EditPersonalAccountAsync(Account);
                RemoteWorkstationConnectionsService.StartUpdateRemoteDevice(await EmployeeService.GetEmployeeVaultIdsAsync(Account.EmployeeId));
                ToastService.ShowToast("Account updated.", ToastLevel.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.EmployeesDetails, Account.EmployeeId, Account.Id);
                await ModalDialogService.CloseAsync();
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(Account.Name), ex.Message);
            }
            catch (IncorrectUrlException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(Account.Urls), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
            finally
            {
                _isBusy = false;
            }
        }
             
        private async Task OnCancelAsync()
        {
            await EmployeeService.UnchangedPersonalAccountAsync(Account);
            ModalDialogService.OnClose -= OnCancelAsync;
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Account.Id);
        }
    }
}