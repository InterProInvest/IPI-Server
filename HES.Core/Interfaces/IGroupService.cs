using HES.Core.Entities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IGroupService
    {
        IQueryable<Group> Query();
        Task<IList<Group>> GetAllGroupsAsync(int skip, int take, ListSortDirection sortDirection = ListSortDirection.Descending, string search = null, string orderBy = nameof(Group.Name));
        Task<Group> GetGroupByIdAsync(string groupId);
        Task<int> GetCountAsync(string search = null);
        Task<Group> CreateGroupAsync(Group group);
        Task CreateGroupRangeAsync(List<Group> groups);
        Task EditGroupAsync(Group group);
        Task DeleteGroupAsync(string groupId);
        Task<List<GroupMembership>> GetGruopMembersAsync(string groupId);
        Task<GroupMembership> GetGroupMembershipAsync(string employeeId, string groupId);
        Task<List<Employee>> GetEmployeesSkipExistingOnesInGroupAsync(string groupId);
        Task AddEmployeesToGroupAsync(IList<string> employeeIds, string groupId);
        Task AddEmployeeToGroupsAsync(string employeeId, IList<string> groupIds);
        Task RemoveEmployeeFromGroupAsync(string groupMembershipId);
    }
}
