using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class DeleteHardwareVault : ComponentBase, IDisposable
    {
        [Inject] IEmployeeService EmployeeService { get; set; }
        [Inject] IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] ILogger<DeleteHardwareVault> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string HardwareVaultId { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }
        public HardwareVault HardwareVault { get; set; }
        public VaultStatusReason Reason { get; set; } = VaultStatusReason.Withdrawal;
        public bool IsNeedBackup { get; set; }
        public bool EntityBeingEdited { get; set; }
        public bool Initialized { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                HardwareVault = await HardwareVaultService.GetVaultByIdAsync(HardwareVaultId);
                if (HardwareVault == null)
                    throw new Exception("HardwareVault not found.");

                EntityBeingEdited = MemoryCache.TryGetValue(HardwareVault.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(HardwareVault.Id, HardwareVault);

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        public async Task DeleteVaultAsync()
        {
            try
            {
                var employeeId = HardwareVault.EmployeeId;
                await EmployeeService.RemoveHardwareVaultAsync(HardwareVault.Id, Reason, IsNeedBackup);
                RemoteWorkstationConnectionsService.StartUpdateRemoteDevice(HardwareVault.Id);
                await Refresh.InvokeAsync(this);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.EmployeesDetails, employeeId, null);
                await HubContext.Clients.All.SendAsync(RefreshPage.HardwareVaultStateChanged, HardwareVault.Id);
                ToastService.ShowToast("Vault removed.", ToastLevel.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        public async Task CancelAsync()
        {
            await ModalDialogService.CloseAsync();
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(HardwareVault.Id);
        }
    }
}