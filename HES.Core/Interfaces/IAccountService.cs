using HES.Core.Entities;
using Hideez.SDK.Communication.PasswordManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IAccountService : IDisposable
    {
        IQueryable<Account> Query();
        Task ReloadAccountAsync(string accountId);
        Task<Account> GetAccountByIdAsync(string accountId);
        Task<Account> GetAccountByIdNoTrackingAsync(string accountId);
        Task<Account> AddAsync(Account account);
        Task<IList<Account>> AddRangeAsync(IList<Account> accounts);
        Task UnchangedAsync(Account account);
        Task UpdateOnlyPropAsync(Account account, string[] properties);
        Task UpdateOnlyPropAsync(IList<Account> accounts, string[] properties);
        Task UpdateAfterAccountCreateAsync(Account account, uint timestamp);
        Task UpdateAfterAccountModifyAsync(Account account, uint timestamp);
        Task DeleteAsync(Account account);
        Task DeleteRangeAsync(IList<Account> accounts);
        Task DeleteAccountsByEmployeeIdAsync(string employeeId);
        Task<bool> ExistAsync(Expression<Func<Account, bool>> predicate);
        Task<StorageId> GetStorageIdAsync(string accountId);
    }
}