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
    public class HardwareVaultTaskService : IHardwareVaultTaskService
    {
        private readonly IAsyncRepository<HardwareVaultTask> _hardwareVaultTaskRepository;

        public HardwareVaultTaskService(IAsyncRepository<HardwareVaultTask> hardwareVaultTaskRepository)
        {
            _hardwareVaultTaskRepository = hardwareVaultTaskRepository;
        }

        public IQueryable<HardwareVaultTask> TaskQuery()
        {
            return _hardwareVaultTaskRepository.Query();
        }

        public async Task<HardwareVaultTask> GetTaskByIdAsync(string taskId)
        {
            return await _hardwareVaultTaskRepository
               .Query()
               .Include(d => d.Account.Employee.HardwareVaults)
               .FirstOrDefaultAsync(d => d.Id == taskId);
        }

        public async Task AddTaskAsync(HardwareVaultTask vaultTask)
        {
            await _hardwareVaultTaskRepository.AddAsync(vaultTask);
        }

        public async Task AddRangeTasksAsync(IList<HardwareVaultTask> vaultTasks)
        {
            await _hardwareVaultTaskRepository.AddRangeAsync(vaultTasks);
        }

        public async Task AddPrimaryAsync(string vaultId, string accountId)
        {
            var task = new HardwareVaultTask()
            {
                Operation = TaskOperation.Primary,
                CreatedAt = DateTime.UtcNow,
                HardwareVaultId = vaultId,
                AccountId = accountId
            };
            await _hardwareVaultTaskRepository.AddAsync(task);
        }

        public async Task AddLinkAsync(string vaultId, string masterPassword)
        {
            var task = new HardwareVaultTask()
            {
                Password = masterPassword,
                Operation = TaskOperation.Link,
                CreatedAt = DateTime.UtcNow,
                HardwareVaultId = vaultId
            };
            await _hardwareVaultTaskRepository.AddAsync(task);
        }

        public async Task AddProfileAsync(HardwareVault vault)
        {
            var previousProfileTask = await _hardwareVaultTaskRepository
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
                    await _hardwareVaultTaskRepository.DeleteAsync(previousProfileTask);
                }

                await _hardwareVaultTaskRepository.AddAsync(newProfileTask);

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

            await _hardwareVaultTaskRepository.AddAsync(task);
        }

        public async Task AddWipeAsync(string vaultId, string masterPassword)
        {
            var task = new HardwareVaultTask()
            {
                Password = masterPassword,
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Wipe,
                HardwareVaultId = vaultId
            };
            await _hardwareVaultTaskRepository.AddAsync(task);
        }

        public async Task UpdateOnlyPropAsync(HardwareVaultTask vaultTask, string[] properties)
        {
            await _hardwareVaultTaskRepository.UpdateOnlyPropAsync(vaultTask, properties);
        }

        public async Task DeleteTaskAsync(HardwareVaultTask vaultTask)
        {
            await _hardwareVaultTaskRepository.DeleteAsync(vaultTask);
        }

        public async Task DeleteTasksByVaultIdAsync(string vaultId)
        {
            var allTasks = await _hardwareVaultTaskRepository
                .Query()
                .Where(t => t.HardwareVaultId == vaultId)
                .ToListAsync();

            await _hardwareVaultTaskRepository.DeleteRangeAsync(allTasks);
        }

        public async Task RemoveAllProfileTasksAsync(string vaultId)
        {
            var tasks = await _hardwareVaultTaskRepository
                .Query()
                .Where(t => t.HardwareVaultId == vaultId && t.Operation == TaskOperation.Profile)
                .ToListAsync();

            await _hardwareVaultTaskRepository.DeleteRangeAsync(tasks);
        }
    }
}