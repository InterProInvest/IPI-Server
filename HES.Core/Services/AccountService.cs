using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAsyncRepository<Account> _accountRepository;

        public AccountService(IAsyncRepository<Account> accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public IQueryable<Account> Query()
        {
            return _accountRepository.Query();
        }

        public Task<Account> GetByIdAsync(string accountId)
        {
            return _accountRepository.GetByIdAsync(accountId);
        }

        public Task<Account> AddAsync(Account deviceAccount)
        {
            return _accountRepository.AddAsync(deviceAccount);
        }

        public Task<IList<Account>> AddRangeAsync(IList<Account> deviceAccounts)
        {
            return _accountRepository.AddRangeAsync(deviceAccounts);
        }

        public Task UpdateOnlyPropAsync(Account deviceAccount, string[] properties)
        {
            return _accountRepository.UpdateOnlyPropAsync(deviceAccount, properties);
        }

        public Task UpdateOnlyPropAsync(IList<Account> deviceAccounts, string[] properties)
        {
            return _accountRepository.UpdateOnlyPropAsync(deviceAccounts, properties);
        }

        public Task DeleteAsync(Account deviceAccount)
        {
            return _accountRepository.DeleteAsync(deviceAccount);
        }

        public Task DeleteRangeAsync(IList<Account> deviceAccounts)
        {
            return _accountRepository.DeleteRangeAsync(deviceAccounts);
        }

        public async Task RemoveAllAccountsByEmployeeIdAsync(string employeeId)
        {
            var allAccounts = await _accountRepository
                .Query()
                .Where(t => t.EmployeeId == employeeId)
                .ToListAsync();

            foreach (var account in allAccounts)
            {
                account.Deleted = true;
            }

            await _accountRepository.UpdateOnlyPropAsync(allAccounts, new string[] { nameof(Account.Deleted) });
        }

        public async Task RemoveAllAccountsAsync(string employeeId)
        {
            var allAccounts = await _accountRepository
                 .Query()
                 .Where(d => d.EmployeeId == employeeId && d.Deleted == false)
                 .ToListAsync();

            foreach (var account in allAccounts)
            {
                account.Deleted = true;
            }

            await _accountRepository.UpdateOnlyPropAsync(allAccounts, new string[] { nameof(Account.Deleted) });
        }

        public async Task<bool> ExistAsync(Expression<Func<Account, bool>> predicate)
        {
            return await _accountRepository.ExistAsync(predicate);
        }
    }
}