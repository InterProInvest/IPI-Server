using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class DeleteHardwareVault : ComponentBase
    {
        [Inject] IEmployeeService EmployeeService { get; set; }
        [Inject] IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] ILogger<DeleteHardwareVault> Logger { get; set; }
        [Parameter] public HardwareVault HardwareVault { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }
        public VaultStatusReason DeletedReason { get; set; } = VaultStatusReason.Withdrawal;

        public async Task DeleteVaultAsync()
        {
            try
            {
                await EmployeeService.RemoveHardwareVaultAsync(HardwareVault.Id, DeletedReason);
                RemoteWorkstationConnectionsService.StartUpdateRemoteDevice(HardwareVault.Id);
                await ModalDialogService.CloseAsync();
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Vault removed.", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                await ModalDialogService.CloseAsync();
                Logger.LogError(ex.Message, ex);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }

        public async Task CancelAsync()
        {
            await ModalDialogService.CloseAsync();
        }
    }
}
