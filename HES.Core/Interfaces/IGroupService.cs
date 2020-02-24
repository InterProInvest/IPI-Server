using HES.Core.Entities;
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
        Task AddEmployeesToGroupAsync(IList<string> employeeIds, string groupId);
        Task AddGroupsToEmployeeAsync(IList<string> groupIds, string employeeId);
        Task<bool> CheckGroupNameAsync(string name);
    }
}