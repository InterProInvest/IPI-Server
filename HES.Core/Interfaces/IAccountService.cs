using HES.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IAccountService
    {
        IQueryable<Account> Query();
        Task<Account> GetAccountByIdAsync(string accountId);
        Task<Account> AddAsync(Account account);
        Task<IList<Account>> AddRangeAsync(IList<Account> accounts);
        Task UnchangedAsync(Account account);
        Task UpdateOnlyPropAsync(Account account, string[] properties);
        Task UpdateOnlyPropAsync(IList<Account> accounts, string[] properties);
        Task UpdateAfterAccountCreationAsync(Account account, uint storageId, uint timestamp);
        Task DeleteAsync(Account account);
        Task DeleteRangeAsync(IList<Account> accounts);
        Task DeleteAccountsByEmployeeIdAsync(string employeeId);
        Task<bool> ExistAsync(Expression<Func<Account, bool>> predicate);
    }
}