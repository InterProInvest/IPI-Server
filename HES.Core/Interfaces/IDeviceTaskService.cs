using HES.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceTaskService
    {
        IQueryable<DeviceTask> TaskQuery();
        Task<DeviceTask> GetTaskByIdAsync(string id);
        Task AddTaskAsync(DeviceTask deviceTask);
        Task AddRangeTasksAsync(IList<DeviceTask> deviceTasks);
        Task AddPrimaryAsync(string deviceId, string accountId);
        Task AddLinkAsync(string deviceId, string masterPassword);
        Task AddProfileAsync(Device device);
        Task AddSuspendAsync(string vaultId);
        Task AddWipeAsync(string deviceId, string masterPassword);
        Task UpdateOnlyPropAsync(DeviceTask deviceTask, string[] properties);
        Task DeleteTaskAsync(DeviceTask deviceTask);
        Task RemoveAllTasksAsync(string deviceId);
        Task RemoveAllProfileTasksAsync(string deviceId);
    }
}
