using HES.Core.Entities;
using HES.Core.Models.Web.Group;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IGroupService
    {
        IQueryable<Group> Query();
        Task<Group> GetGroupByIdAsync(string groupId);
        Task<List<Group>> GetGroupsAsync();
        Task<Group> CreateGroupAsync(Group group);
        Task EditGroupAsync(Group group);
        Task DeleteGroupAsync(string groupId);
        Task<List<GroupMembership>> GetGruopMembersAsync(string groupId);
        Task<List<GroupEmployee>> GetMappedGroupEmployeesAsync(string groupId);
        Task AddEmployeesToGroupAsync(IList<string> employeeIds, string groupId);
        Task ManageEmployeesAsync(List<GroupEmployee> groupEmployees, string groupId);
        Task<bool> CheckGroupNameAsync(string name);
    }
}