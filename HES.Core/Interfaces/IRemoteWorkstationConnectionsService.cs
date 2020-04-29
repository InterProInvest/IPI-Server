using System.Collections.Generic;
using System.Threading.Tasks;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Workstation;

namespace HES.Core.Interfaces
{
    public interface IRemoteWorkstationConnectionsService
    {
        void StartUpdateRemoteDevice(IList<string> vaultIds);
        void StartUpdateRemoteDevice(string vaultId);
        Task UpdateRemoteDeviceAsync(string vaultId, string workstationId, bool primaryAccountOnly);
        Task RegisterWorkstationInfoAsync(IRemoteAppConnection remoteAppConnection, WorkstationInfo workstationInfo);
        Task OnAppHubDisconnectedAsync(string workstationId);
    }
}
