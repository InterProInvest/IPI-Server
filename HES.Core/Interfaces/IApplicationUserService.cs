using HES.Core.Entities;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Users;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IApplicationUserService
    {
        IQueryable<ApplicationUser> Query();
        Task DetachUserAsync(ApplicationUser user);
        Task DetachUsersAsync(List<ApplicationUser> users);
        Task<List<ApplicationUser>> GetAdministratorsAsync(DataLoadingOptions<ApplicationUserFilter> dataLoadingOptions);
        Task<int> GetAdministratorsCountAsync(DataLoadingOptions<ApplicationUserFilter> dataLoadingOptions);
        Task<string> InviteAdministratorAsync(string email, string domain);
        Task<string> GetCallBackUrl(string email, string domain);
        Task<ApplicationUser> GetUserByIdAsync(string id);
        Task<ApplicationUser> DeleteUserAsync(string id);
        Task<IList<ApplicationUser>> GetAllAsync();
        Task<IList<ApplicationUser>> GetAdministratorsAsync();
    }
}