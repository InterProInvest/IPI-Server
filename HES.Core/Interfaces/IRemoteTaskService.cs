using System.Threading.Tasks;
using HES.Core.Entities;
using Hideez.SDK.Communication.Remote;

namespace HES.Core.Interfaces
{
    public interface IRemoteTaskService
    {
        Task ExecuteRemoteTasks(string vaultId, RemoteDevice remoteDevice, bool primaryAccountOnly);
        Task LinkVaultAsync(RemoteDevice remoteDevice, HardwareVault vault);
        Task SuspendVaultAsync(RemoteDevice remoteDevice, HardwareVault vault);
        Task WipeVaultAsync(RemoteDevice remoteDevice, HardwareVault vault);
    }
}