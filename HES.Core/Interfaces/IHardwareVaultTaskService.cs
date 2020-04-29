using HES.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IHardwareVaultTaskService
    {
        IQueryable<HardwareVaultTask> TaskQuery();
        Task<HardwareVaultTask> GetTaskByIdAsync(string id);
        Task AddTaskAsync(HardwareVaultTask vaultTask);
        Task AddRangeTasksAsync(IList<HardwareVaultTask> vaultTasks);
        Task AddPrimaryAsync(string vaultId, string accountId);
        Task AddLinkAsync(string vaultId, string masterPassword);
        Task AddProfileAsync(HardwareVault vault);
        Task AddSuspendAsync(string vaultId);
        Task AddWipeAsync(string vaultId, string masterPassword);
        Task UpdateOnlyPropAsync(HardwareVaultTask vaultTask, string[] properties);
        Task DeleteTaskAsync(HardwareVaultTask vaultTask);
        Task RemoveAllTasksAsync(string vaultId);
        Task RemoveAllProfileTasksAsync(string vaultId);
    }
}
