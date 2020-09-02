using System;
using System.Threading.Tasks;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Remote;

namespace HES.Core.Interfaces
{
    public interface IRemoteDeviceConnectionsService : IDisposable
    {
        void OnDeviceHubConnected(string deviceId, string workstationId, IRemoteCommands caller);
        void OnDeviceHubDisconnected(string deviceId, string workstationId);

        void OnAppHubConnected(string workstationId, IRemoteAppConnection appConnection);
        void OnAppHubDisconnected(string workstationId);

        // Received via AppHub
        void OnDeviceConnected(string deviceId, string workstationId, IRemoteAppConnection appConnection);
        void OnDeviceDisconnected(string deviceId, string workstationId);

        Task<RemoteDevice> ConnectDevice(string deviceId, string workstationId);
        RemoteDevice FindRemoteDevice(string deviceId, string workstationId);

        Task SyncHardwareVaults(string vaultId);
    }
}
