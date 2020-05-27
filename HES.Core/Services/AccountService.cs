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

        public Task<Account> GetAccountByIdAsync(string accountId)
        {
            return _accountRepository
                .Query()
                .Include(x => x.Employee)
                .Include(x => x.SharedAccount)
                .FirstOrDefaultAsync(x => x.Id == accountId);
        }

        public Task<Account> AddAsync(Account account)
        {
            return _accountRepository.AddAsync(account);
        }

        public Task<IList<Account>> AddRangeAsync(IList<Account> accounts)
        {
            return _accountRepository.AddRangeAsync(accounts);
        }

        public Task UnchangedAsync(Account account)
        {
            return _accountRepository.Unchanged(account);
        }

        public async Task UpdateOnlyPropAsync(Account account, string[] properties)
        {
            await _accountRepository.UpdateOnlyPropAsync(account, properties);
        }

        public async Task UpdateOnlyPropAsync(IList<Account> accounts, string[] properties)
        {
            await _accountRepository.UpdateOnlyPropAsync(accounts, properties);
        }

        public async Task UpdateAfterAccountCreationAsync(Account account, uint storageId, uint timestamp)
        {
            account.StorageId = storageId;
            account.Timestamp = timestamp;
            account.Password = null;
            account.OtpSecret = null;
            await _accountRepository.UpdateAsync(account);
        }

        public Task DeleteAsync(Account account)
        {
            return _accountRepository.DeleteAsync(account);
        }

        public Task DeleteRangeAsync(IList<Account> accounts)
        {
            return _accountRepository.DeleteRangeAsync(accounts);
        }

        public async Task DeleteAccountsByEmployeeIdAsync(string employeeId)
        {
            var accounts = await _accountRepository
                   .Query()
                   .Where(x => x.EmployeeId == employeeId && x.Deleted == false)
                   .ToListAsync();

            foreach (var account in accounts)
            {
                account.Deleted = true;
            }

            await _accountRepository.UpdateOnlyPropAsync(accounts, new string[] { nameof(Account.Deleted) });
        }

        public async Task<bool> ExistAsync(Expression<Func<Account, bool>> predicate)
        {
            return await _accountRepository.ExistAsync(predicate);
        }
    }
}