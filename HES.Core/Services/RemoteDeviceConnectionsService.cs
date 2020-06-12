using HES.Core.Enums;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class RemoteDeviceConnectionsService : IRemoteDeviceConnectionsService
    {
        static readonly ConcurrentDictionary<string, DeviceRemoteConnections> _deviceRemoteConnectionsList
            = new ConcurrentDictionary<string, DeviceRemoteConnections>();
        private readonly IHardwareVaultService _hardwareVaultService;
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<RemoteDeviceConnectionsService> _logger;

        public RemoteDeviceConnectionsService(IHardwareVaultService hardwareVaultService, IEmployeeService employeeService, ILogger<RemoteDeviceConnectionsService> logger)
        {
            _hardwareVaultService = hardwareVaultService;
            _employeeService = employeeService;
            _logger = logger;
        }

        static DeviceRemoteConnections GetDeviceRemoteConnections(string deviceId)
        {
            return _deviceRemoteConnectionsList.GetOrAdd(deviceId, (x) =>
            {
                return new DeviceRemoteConnections(deviceId);
            });
        }

        // Device connected to the host (called by AppHub)
        public void OnDeviceConnected(string deviceId, string workstationId, IRemoteAppConnection appConnection)
        {
            GetDeviceRemoteConnections(deviceId).OnDeviceConnected(workstationId, appConnection);
        }

        // Device disconnected from the host (called by AppHub)
        public void OnDeviceDisconnected(string deviceId, string workstationId)
        {
            GetDeviceRemoteConnections(deviceId).OnDeviceDisconnected(workstationId);
        }

        // Device hub connected. That means we need to create RemoteDevice
        public void OnDeviceHubConnected(string deviceId, string workstationId, IRemoteCommands caller)
        {
            GetDeviceRemoteConnections(deviceId).OnDeviceHubConnected(workstationId, caller);
        }

        // Device hub disconnected
        public void OnDeviceHubDisconnected(string deviceId, string workstationId)
        {
            GetDeviceRemoteConnections(deviceId).OnDeviceHubDisconnected(workstationId);
        }

        // AppHub connected
        public void OnAppHubConnected(string workstationId, IRemoteAppConnection appConnection)
        {
        }

        // AppHub disconnected
        public void OnAppHubDisconnected(string workstationId)
        {
            foreach (var item in _deviceRemoteConnectionsList.Values)
            {
                item.OnAppHubDisconnected(workstationId);
            }
        }

        public static bool IsDeviceConnectedToHost(string deviceId)
        {
            return GetDeviceRemoteConnections(deviceId).IsDeviceConnectedToHost;
        }

        public Task<RemoteDevice> ConnectDevice(string deviceId, string workstationId)
        {
            _deviceRemoteConnectionsList.TryGetValue(deviceId, out DeviceRemoteConnections deviceRemoteConnections);
            if (deviceRemoteConnections == null || !deviceRemoteConnections.IsDeviceConnectedToHost)
                throw new HideezException(HideezErrorCode.DeviceNotConnectedToAnyHost);

            return deviceRemoteConnections.ConnectDevice(workstationId);
        }

        public RemoteDevice FindRemoteDevice(string deviceId, string workstationId)
        {
            return GetDeviceRemoteConnections(deviceId).GetRemoteDevice(workstationId);
        }

        public async Task SyncHardwareVaults(string vaultId)
        {
            try
            {
                var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);

                if (vault.Status != VaultStatus.Active || !vault.NeedSync || vault.EmployeeId == null)
                    return;

                var employee = await _employeeService.GetEmployeeByIdAsync(vault.EmployeeId);

                var employeeVaults = employee.HardwareVaults.Where(x => x.Id != vaultId && x.IsOnline && x.Timestamp != vault.Timestamp && x.Status == VaultStatus.Active && !x.NeedSync).ToList();
                foreach (var employeeVault in employeeVaults)
                {
                    var firstWorkstationId = GetDeviceRemoteConnections(vault.Id).GetFirstOrDefaultWorkstation();
                    var secondWorkstationId = GetDeviceRemoteConnections(employeeVault.Id).GetFirstOrDefaultWorkstation();

                    if (firstWorkstationId == null || secondWorkstationId == null)
                        return;

                    var firstRemoteDeviceTask = ConnectDevice(vault.Id, firstWorkstationId).TimeoutAfter(30_000);
                    var secondRemoteDeviceTask = ConnectDevice(employeeVault.Id, secondWorkstationId).TimeoutAfter(30_000);
                    await Task.WhenAll(firstRemoteDeviceTask, secondRemoteDeviceTask);

                    if (firstRemoteDeviceTask.Result == null || secondRemoteDeviceTask.Result == null)
                        return;

                    await new DeviceStorageReplicator(firstRemoteDeviceTask.Result, secondRemoteDeviceTask.Result, null).Start();

                    if (vault.Timestamp > employeeVault.Timestamp)
                    {
                        employeeVault.Timestamp = vault.Timestamp;
                        await _hardwareVaultService.UpdateVaultAsync(employeeVault);
                    }
                    else
                    {
                        vault.Timestamp = employeeVault.Timestamp;
                        await _hardwareVaultService.UpdateVaultAsync(vault);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Sync Hardware Vaults - {ex.Message}");
            }
        }
    }
}