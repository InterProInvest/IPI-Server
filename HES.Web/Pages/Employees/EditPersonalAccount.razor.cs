using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class EditPersonalAccount : ComponentBase
    {
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EditPersonalAccount> Logger { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }
        [Parameter] public Account Account { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }

        private bool _isBusy;

        protected override void OnInitialized()
        {
            ModalDialogService.OnClose += ModalDialogService_OnClose;
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
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Account updated.", ToastLevel.Success);
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
                await CloseAsync();
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async Task CloseAsync()
        {
            await EmployeeService.UnchangedPersonalAccountAsync(Account);
            await ModalDialogService.CloseAsync();
        }

        private async Task ModalDialogService_OnClose()
        {
            await EmployeeService.UnchangedPersonalAccountAsync(Account);
            ModalDialogService.OnClose -= ModalDialogService_OnClose;
        }
    }
}