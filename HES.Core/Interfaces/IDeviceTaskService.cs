using HES.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceTaskService
    {
        IQueryable<DeviceTask> Query();
        Task AddTaskAsync(DeviceTask deviceTask);
        Task AddRangeTasksAsync(IList<DeviceTask> deviceTasks);
        Task AddLinkAsync(string deviceId, string masterPassword);
        Task AddProfileAsync(Device device);
        Task AddUnlockPinAsync(Device device);
        Task AddWipeAsync(string deviceId, string masterPassword);
        Task UpdateOnlyPropAsync(DeviceTask deviceTask, string[] properties);
        Task UndoLastTaskAsync(string accountId);
        Task DeleteTaskAsync(DeviceTask deviceTask);
        Task RemoveAllTasksAsync(string deviceId);
        Task RemoveAllProfileTasksAsync(string deviceId);
    }
}
