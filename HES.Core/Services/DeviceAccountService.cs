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

        public Task<DeviceAccount> GetByIdAsync(string accountId)
        {
            return _deviceAccountRepository.GetByIdAsync(accountId);
        }

        public Task<DeviceAccount> AddAsync(DeviceAccount deviceAccount)
        {
            return _deviceAccountRepository.AddAsync(deviceAccount);
        }

        public Task<IList<DeviceAccount>> AddRangeAsync(IList<DeviceAccount> deviceAccounts)
        {
            return _deviceAccountRepository.AddRangeAsync(deviceAccounts);
        }

        public Task UpdateOnlyPropAsync(DeviceAccount deviceAccount, string[] properties)
        {
            return _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);
        }

        public Task UpdateOnlyPropAsync(IList<DeviceAccount> deviceAccounts, string[] properties)
        {
            return _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccounts, properties);
        }

        public Task DeleteAsync(DeviceAccount deviceAccount)
        {
            return _deviceAccountRepository.DeleteAsync(deviceAccount);
        }

        public Task DeleteRangeAsync(IList<DeviceAccount> deviceAccounts)
        {
            return _deviceAccountRepository.DeleteRangeAsync(deviceAccounts);
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