using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Remote;
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

        public RemoteDeviceConnectionsService(IHardwareVaultService hardwareVaultService)
        {
            _hardwareVaultService = hardwareVaultService;
        }

        static DeviceRemoteConnections GetDeviceRemoteConnections(string deviceId)
        {
            return _deviceRemoteConnectionsList.GetOrAdd(deviceId, (x) =>
            {
                return new DeviceRemoteConnections(deviceId);
            });
        }

        // Device connected to the host (called by AppHub)
        public async Task OnDeviceConnected(string deviceId, string workstationId, IRemoteAppConnection appConnection)
        {
            GetDeviceRemoteConnections(deviceId).OnDeviceConnected(workstationId, appConnection);
            await SyncHardwareVaults(deviceId);
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

        private async Task SyncHardwareVaults(string vaultId)
        {
            var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);
            var employeeVaults = vault.Employee.HardwareVaults.Where(x => x.Id != vaultId && x.IsOnline && x.Timestamp != vault.Timestamp).ToList();
            foreach (var employeeVault in employeeVaults)
            {
                var firstRemoteDevice = GetDeviceRemoteConnections(vault.Id).GetDefaultRemoteDevice();
                var secondRemoteDevice = GetDeviceRemoteConnections(employeeVault.Id).GetDefaultRemoteDevice();

                await new DeviceStorageReplicator(firstRemoteDevice, secondRemoteDevice, null).Start();
            }
        }
    }
}