using HES.Core.Entities;
using HES.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ISharedAccountService
    {
        IQueryable<SharedAccount> Query();
        Task<SharedAccount> GetByIdAsync(dynamic id);
        Task<List<SharedAccount>> GetSharedAccountsAsync();
        Task<SharedAccount> CreateSharedAccountAsync(SharedAccount sharedAccount);
        Task<SharedAccount> CreateWorkstationSharedAccountAsync(WorkstationAccount workstationAccount);
        Task<List<string>> EditSharedAccountAsync(SharedAccount sharedAccount);
        Task<List<string>> EditSharedAccountPwdAsync(SharedAccount sharedAccount);
        Task<List<string>> EditSharedAccountOtpAsync(SharedAccount sharedAccount);
        Task<List<string>> DeleteSharedAccountAsync(string id);
        Task<bool> ExistAync(Expression<Func<SharedAccount, bool>> predicate);
    }
}