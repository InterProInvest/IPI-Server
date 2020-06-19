using HES.Core.Entities;
using HES.Core.Models;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Account;
using HES.Core.Models.Web.SharedAccounts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ISharedAccountService
    {
        IQueryable<SharedAccount> Query();
        Task<SharedAccount> GetSharedAccountByIdAsync(string id);
        Task<List<SharedAccount>> GetSharedAccountsAsync(DataLoadingOptions<SharedAccountsFilter> dataLoadingOptions);
        Task<int> GetSharedAccountsCountAsync(DataLoadingOptions<SharedAccountsFilter> dataLoadingOptions);
        Task<List<SharedAccount>> GetWorkstationSharedAccountsAsync();
        Task<SharedAccount> CreateSharedAccountAsync(SharedAccount sharedAccount);
        Task<SharedAccount> CreateWorkstationSharedAccountAsync(WorkstationSharedAccount workstationAccount);
        Task<SharedAccount> CreateWorkstationSharedAccountAsync(WorkstationDomainSharedAccount workstationAccount);
        Task<List<string>> EditSharedAccountAsync(SharedAccount sharedAccount);
        Task<List<string>> EditSharedAccountPwdAsync(SharedAccount sharedAccount, AccountPassword accountPassword);
        Task<List<string>> EditSharedAccountOtpAsync(SharedAccount sharedAccount, AccountOtp accountOtp);
        Task<List<string>> DeleteSharedAccountAsync(string id);
        Task DetachSharedAccountAsync(SharedAccount sharedAccount);
        Task DetachSharedAccountAsync(List<SharedAccount> sharedAccounts);

    }
}