using HES.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IApplicationUserService
    {
        IQueryable<ApplicationUser> Query();
        Task<ApplicationUser> GetUserByIdAsync(string id);
        Task<ApplicationUser> DeleteUserAsync(string id);
        Task<IList<ApplicationUser>> GetAllAsync();
        Task<IList<ApplicationUser>> GetAdministratorsAsync();
    }
}