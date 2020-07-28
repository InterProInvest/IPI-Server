using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
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
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public HardwareVault HardwareVault { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }
        public VaultStatusReason Reason { get; set; } = VaultStatusReason.Withdrawal;
        public bool IsNeedBackup { get; set; }

        public async Task DeleteVaultAsync()
        {
            try
            {
                var employeeId = HardwareVault.EmployeeId;
                await EmployeeService.RemoveHardwareVaultAsync(HardwareVault.Id, Reason, IsNeedBackup);
                RemoteWorkstationConnectionsService.StartUpdateRemoteDevice(HardwareVault.Id);
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Vault removed.", ToastLevel.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.EmployeesDetails, employeeId, null);
                await HubContext.Clients.All.SendAsync(RefreshPage.HardwareVaultStateChanged, HardwareVault.Id);
                await ModalDialogService.CloseAsync();
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
