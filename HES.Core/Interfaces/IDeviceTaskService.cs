using HES.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceTaskService
    {
        IQueryable<HardwareVaultTask> TaskQuery();
        Task<HardwareVaultTask> GetTaskByIdAsync(string id);
        Task AddTaskAsync(HardwareVaultTask deviceTask);
        Task AddRangeTasksAsync(IList<HardwareVaultTask> deviceTasks);
        Task AddPrimaryAsync(string deviceId, string accountId);
        Task AddLinkAsync(string deviceId, string masterPassword);
        Task AddProfileAsync(HardwareVault device);
        Task AddSuspendAsync(string vaultId);
        Task AddWipeAsync(string deviceId, string masterPassword);
        Task UpdateOnlyPropAsync(HardwareVaultTask deviceTask, string[] properties);
        Task DeleteTaskAsync(HardwareVaultTask deviceTask);
        Task RemoveAllTasksAsync(string deviceId);
        Task RemoveAllProfileTasksAsync(string deviceId);
    }
}
