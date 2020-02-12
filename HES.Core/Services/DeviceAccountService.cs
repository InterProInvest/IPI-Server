using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DeviceAccountService : IDeviceAccountService
    {
        private readonly IAsyncRepository<DeviceAccount> _deviceAccountRepository;

        public DeviceAccountService(IAsyncRepository<DeviceAccount> deviceAccountRepository)
        {
            _deviceAccountRepository = deviceAccountRepository;
        }

        public IQueryable<DeviceAccount> Query()
        {
            return _deviceAccountRepository.Query();
        }

        public async Task<DeviceAccount> GetByIdAsync(string accountId)
        {
            return await _deviceAccountRepository.GetByIdAsync(accountId);
        }

        public async Task<DeviceAccount> AddAsync(DeviceAccount deviceAccount)
        {
            return await _deviceAccountRepository.AddAsync(deviceAccount);
        }

        public async Task<IList<DeviceAccount>> AddRangeAsync(IList<DeviceAccount> deviceAccounts)
        {
            return await _deviceAccountRepository.AddRangeAsync(deviceAccounts);
        }

        public async Task UpdateOnlyPropAsync(DeviceAccount deviceAccount, string[] properties)
        {
            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);
        }

        public async Task UpdateOnlyPropAsync(IList<DeviceAccount> deviceAccounts, string[] properties)
        {
            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccounts, properties);
        }

        public async Task DeleteAsync(DeviceAccount deviceAccount)
        {
            await _deviceAccountRepository.DeleteAsync(deviceAccount);
        }

        public async Task DeleteRangeAsync(IList<DeviceAccount> deviceAccounts)
        {
            await _deviceAccountRepository.DeleteRangeAsync(deviceAccounts);
        }

        public async Task RemoveAllAccountsByEmployeeIdAsync(string employeeId)
        {
            var allAccounts = await _deviceAccountRepository
                .Query()
                .Where(t => t.EmployeeId == employeeId)
                .ToListAsync();

            foreach (var account in allAccounts)
            {
                account.Deleted = true;
            }

            await _deviceAccountRepository.UpdateOnlyPropAsync(allAccounts, new string[] { "Deleted" });
        }

        public async Task RemoveAllAccountsAsync(string deviceId)
        {
            var allAccounts = await _deviceAccountRepository
                 .Query()
                 .Where(d => d.DeviceId == deviceId && d.Deleted == false)
                 .ToListAsync();

            foreach (var account in allAccounts)
            {
                account.Deleted = true;
            }

            await _deviceAccountRepository.UpdateOnlyPropAsync(allAccounts, new string[] { "Deleted" });
        }
    }
}