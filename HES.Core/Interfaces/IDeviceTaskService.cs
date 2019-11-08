using HES.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceTaskService
    {
        IQueryable<DeviceTask> Query();
        Task<int> GetCountAsync();
        Task AddTaskAsync(DeviceTask deviceTask);
        Task AddProfileTaskAsync(Device device);
        Task AddUnlockPinTaskAsync(Device device);
        Task AddRangeAsync(IList<DeviceTask> deviceTasks);
        Task UpdateOnlyPropAsync(DeviceTask deviceTask, string[] properties);
        Task UndoLastTaskAsync(string accountId);
        Task DeleteTaskAsync(DeviceTask deviceTask);
        Task RemoveAllTasksAsync(string deviceId);
        Task RemoveAllProfileTasksAsync(string deviceId);
    }
}
