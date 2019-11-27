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
        private readonly IDeviceAccountService _deviceAccountService;
        private readonly IAsyncRepository<Device> _deviceRepository;

        public DeviceTaskService(IAsyncRepository<DeviceTask> deviceTaskRepository,
                                 IDeviceAccountService deviceAccountService,
                                 IAsyncRepository<Device> deviceRepository)
        {
            _deviceTaskRepository = deviceTaskRepository;
            _deviceAccountService = deviceAccountService;
            _deviceRepository = deviceRepository;
        }

        public IQueryable<DeviceTask> Query()
        {
            return _deviceTaskRepository.Query();
        }

        public async Task AddTaskAsync(DeviceTask deviceTask)
        {
            await _deviceTaskRepository.AddAsync(deviceTask);
        }

        public async Task AddRangeTasksAsync(IList<DeviceTask> deviceTasks)
        {
            await _deviceTaskRepository.AddRangeAsync(deviceTasks);
        }

        public async Task AddPrimaryAsync(string deviceId, string currentAccountId, string newAccountId)
        {
            var task = new DeviceTask()
            {
                Operation = TaskOperation.Primary,
                CreatedAt = DateTime.UtcNow,
                Login = currentAccountId,
                DeviceId = deviceId,
                DeviceAccountId = newAccountId
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

        public async Task AddUnlockPinAsync(Device device)
        {
            var task = new DeviceTask
            {
                DeviceId = device.Id,
                Password = device.MasterPassword,
                Operation = TaskOperation.UnlockPin,
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

        public async Task<DeviceAccount> GetLastChangedAccountAsync(string deviceId)
        {
            if (deviceId == null)
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            var lastTask = await GetLastChangeTaskAsync(deviceId);

            var currentAccount = await _deviceAccountService
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == lastTask.DeviceAccountId);

            return currentAccount;
        }

        public async Task UndoLastTaskAsync(string deviceId)
        {
            if (deviceId == null)
            {
                throw new ArgumentNullException(nameof(deviceId));

            }

            var properties = new List<string>() { "Status", "UpdatedAt" };

            // Get last task
            var lastTask = await GetLastChangeTaskAsync(deviceId);
            // Current device account
            var deviceAccount = await _deviceAccountService.GetByIdAsync(lastTask.DeviceAccountId);

            switch (lastTask.Operation)
            {
                case TaskOperation.Update:
                    deviceAccount.Name = lastTask.Name;
                    deviceAccount.Urls = lastTask.Urls;
                    deviceAccount.Apps = lastTask.Apps;
                    deviceAccount.Login = lastTask.Login;
                    properties.AddRange(new string[] { "Name", "Urls", "Apps", "Login" });
                    break;
                case TaskOperation.Primary:
                    var device = await _deviceRepository
                        .Query()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(d => d.Id == deviceAccount.DeviceId);
                    device.PrimaryAccountId = lastTask.Login;
                    await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "PrimaryAccountId" });
                    break;
            }

            // Delete last task
            await _deviceTaskRepository.DeleteAsync(lastTask);

            // Get previous task             
            var previousTask = await _deviceTaskRepository
                .Query()
                .Where(d => d.DeviceAccountId == deviceAccount.Id &&
                           (d.Operation == TaskOperation.Create ||
                            d.Operation == TaskOperation.Update ||
                            d.Operation == TaskOperation.Delete ||
                            d.Operation == TaskOperation.Primary))
                .OrderByDescending(d => d.CreatedAt)
                .FirstOrDefaultAsync();

            // If exists previous task for device account, update status
            if (previousTask != null)
            {
                switch (previousTask.Operation)
                {
                    case TaskOperation.Create:
                        deviceAccount.Status = AccountStatus.Creating;
                        deviceAccount.UpdatedAt = null;
                        break;
                    case TaskOperation.Update:
                    case TaskOperation.Primary:
                        deviceAccount.Status = AccountStatus.Updating;
                        deviceAccount.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
            else
            {
                deviceAccount.Status = AccountStatus.Done;
                deviceAccount.UpdatedAt = DateTime.UtcNow;
            }

            await _deviceAccountService.UpdateOnlyPropAsync(deviceAccount, properties.ToArray());
        }

        private async Task<DeviceTask> GetLastChangeTaskAsync(string deviceId)
        {
            if (deviceId == null)
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            return await _deviceTaskRepository
               .Query()
               .Where(d => d.DeviceId == deviceId &&
                          (d.Operation == TaskOperation.Create ||
                           d.Operation == TaskOperation.Update ||
                           d.Operation == TaskOperation.Delete ||
                           d.Operation == TaskOperation.Primary))
               .OrderByDescending(d => d.CreatedAt)
               .FirstOrDefaultAsync();
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
