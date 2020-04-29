using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Core.Services
{
    public class DeviceTaskService : IDeviceTaskService
    {
        private readonly IAsyncRepository<HardwareVaultTask> _deviceTaskRepository;

        public DeviceTaskService(IAsyncRepository<HardwareVaultTask> deviceTaskRepository)
        {
            _deviceTaskRepository = deviceTaskRepository;
        }

        public IQueryable<HardwareVaultTask> TaskQuery()
        {
            return _deviceTaskRepository.Query();
        }

        public async Task<HardwareVaultTask> GetTaskByIdAsync(string id)
        {
            return await _deviceTaskRepository
               .Query()
               .Include(d => d.Account.Employee.HardwareVaults)
               .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddTaskAsync(HardwareVaultTask deviceTask)
        {
            await _deviceTaskRepository.AddAsync(deviceTask);
        }

        public async Task AddRangeTasksAsync(IList<HardwareVaultTask> deviceTasks)
        {
            await _deviceTaskRepository.AddRangeAsync(deviceTasks);
        }

        public async Task AddPrimaryAsync(string deviceId, string accountId)
        {
            var task = new HardwareVaultTask()
            {
                Operation = TaskOperation.Primary,
                CreatedAt = DateTime.UtcNow,
                HardwareVaultId = deviceId,
                AccountId = accountId
            };
            await _deviceTaskRepository.AddAsync(task);
        }

        public async Task AddLinkAsync(string deviceId, string masterPassword)
        {
            var task = new HardwareVaultTask()
            {
                Password = masterPassword,
                Operation = TaskOperation.Link,
                CreatedAt = DateTime.UtcNow,
                HardwareVaultId = deviceId
            };
            await _deviceTaskRepository.AddAsync(task);
        }

        public async Task AddProfileAsync(HardwareVault vault)
        {
            var previousProfileTask = await _deviceTaskRepository
                .Query()
                .FirstOrDefaultAsync(x => x.HardwareVaultId == vault.Id && x.Operation == TaskOperation.Profile);

            var newProfileTask = new HardwareVaultTask
            {
                HardwareVaultId = vault.Id,
                Password = vault.MasterPassword,
                Operation = TaskOperation.Profile,
                CreatedAt = DateTime.UtcNow
            };

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                if (previousProfileTask != null)
                {
                    await _deviceTaskRepository.DeleteAsync(previousProfileTask);
                }

                await _deviceTaskRepository.AddAsync(newProfileTask);

                transactionScope.Complete();
            }
        }

        public async Task AddSuspendAsync(string vaultId)
        {
            var task = new HardwareVaultTask
            {
                HardwareVaultId = vaultId,
                Operation = TaskOperation.Suspend,
                CreatedAt = DateTime.UtcNow
            };

            await _deviceTaskRepository.AddAsync(task);
        }

        public async Task AddWipeAsync(string deviceId, string masterPassword)
        {
            var task = new HardwareVaultTask()
            {
                Password = masterPassword,
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Wipe,
                HardwareVaultId = deviceId
            };
            await _deviceTaskRepository.AddAsync(task);
        }

        public async Task UpdateOnlyPropAsync(HardwareVaultTask deviceTask, string[] properties)
        {
            await _deviceTaskRepository.UpdateOnlyPropAsync(deviceTask, properties);
        }

        public async Task DeleteTaskAsync(HardwareVaultTask deviceTask)
        {
            await _deviceTaskRepository.DeleteAsync(deviceTask);
        }

        public async Task RemoveAllTasksAsync(string deviceId)
        {
            var allTasks = await _deviceTaskRepository
                .Query()
                .Where(t => t.HardwareVaultId == deviceId)
                .ToListAsync();

            await _deviceTaskRepository.DeleteRangeAsync(allTasks);
        }

        public async Task RemoveAllProfileTasksAsync(string vaultId)
        {
            var tasks = await _deviceTaskRepository
                .Query()
                .Where(t => t.HardwareVaultId == vaultId && t.Operation == TaskOperation.Profile)
                .ToListAsync();

            await _deviceTaskRepository.DeleteRangeAsync(tasks);
        }
    }
}