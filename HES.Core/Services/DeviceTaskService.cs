using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DeviceTaskService : IDeviceTaskService
    {
        private readonly IAsyncRepository<DeviceTask> _deviceTaskRepository;

        public DeviceTaskService(IAsyncRepository<DeviceTask> deviceTaskRepository)
        {
            _deviceTaskRepository = deviceTaskRepository;
        }

        public IQueryable<DeviceTask> TaskQuery()
        {
            return _deviceTaskRepository.Query();
        }

        public async Task<DeviceTask> GetTaskByIdAsync(string id)
        {
            return await _deviceTaskRepository
               .Query()
               .Include(d => d.Account.Employee.Devices)
               .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddTaskAsync(DeviceTask deviceTask)
        {
            await _deviceTaskRepository.AddAsync(deviceTask);
        }

        public async Task AddRangeTasksAsync(IList<DeviceTask> deviceTasks)
        {
            await _deviceTaskRepository.AddRangeAsync(deviceTasks);
        }

        public async Task AddPrimaryAsync(string deviceId, string accountId)
        {
            var task = new DeviceTask()
            {
                Operation = TaskOperation.Primary,
                CreatedAt = DateTime.UtcNow,
                DeviceId = deviceId,
                AccountId = accountId
            };
            await _deviceTaskRepository.AddAsync(task);
        }

        public async Task AddLinkAsync(string deviceId, string masterPassword)
        {
            var task = new DeviceTask()
            {
                Password = masterPassword,
                Operation = TaskOperation.Link,
                CreatedAt = DateTime.UtcNow,
                DeviceId = deviceId
            };
            await _deviceTaskRepository.AddAsync(task);
        }

        public async Task AddProfileAsync(Device device)
        {
            var task = new DeviceTask
            {
                DeviceId = device.Id,
                Password = device.MasterPassword,
                Operation = TaskOperation.Profile,
                CreatedAt = DateTime.UtcNow
            };

            await _deviceTaskRepository.AddAsync(task);
        }

        public async Task AddSuspendAsync(string vaultId)
        {
            //TODOSTATUS
            var task = new DeviceTask
            {
                DeviceId = vaultId,   
                Operation = TaskOperation.Suspend,
                CreatedAt = DateTime.UtcNow
            };

            await _deviceTaskRepository.AddAsync(task);
        }

        public async Task AddWipeAsync(string deviceId, string masterPassword)
        {
            var task = new DeviceTask()
            {
                Password = masterPassword,
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Wipe,
                DeviceId = deviceId
            };
            await _deviceTaskRepository.AddAsync(task);
        }

        public async Task UpdateOnlyPropAsync(DeviceTask deviceTask, string[] properties)
        {
            await _deviceTaskRepository.UpdateOnlyPropAsync(deviceTask, properties);
        }

        public async Task DeleteTaskAsync(DeviceTask deviceTask)
        {
            await _deviceTaskRepository.DeleteAsync(deviceTask);
        }

        public async Task RemoveAllTasksAsync(string deviceId)
        {
            var allTasks = await _deviceTaskRepository
                .Query()
                .Where(t => t.DeviceId == deviceId)
                .ToListAsync();

            await _deviceTaskRepository.DeleteRangeAsync(allTasks);
        }

        public async Task RemoveAllProfileTasksAsync(string deviceId)
        {
            var allTasks = await _deviceTaskRepository
                .Query()
                .Where(t => t.DeviceId == deviceId && t.Operation == TaskOperation.Profile)
                .ToListAsync();

            await _deviceTaskRepository.DeleteRangeAsync(allTasks);
        }
    }
}