using HES.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IApplicationUserService
    {
        IQueryable<ApplicationUser> Query();
        Task<ApplicationUser> GetByIdAsync(dynamic id);
        Task<IList<ApplicationUser>> GetAllAsync();
        Task<IList<ApplicationUser>> GetOnlyAdministrators();
        Task DeleteUserAsync(string id);
        Task SendEmailDataProtectionNotify();
    }
}