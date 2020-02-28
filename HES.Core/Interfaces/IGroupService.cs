using System.Linq;
using HES.Core.Entities;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HES.Core.Interfaces
{
    public interface IGroupService
    {
        IQueryable<Group> Query();
        Task<IList<Group>> GetAllGroupsAsync(int skip, int take, ListSortDirection sortDirection = ListSortDirection.Descending, string search = null, string orderBy = nameof(Group.Name));
        Task<Group> GetGroupByIdAsync(string groupId);
        Task<int> GetCountAsync(string search = null);
        Task<Group> CreateGroupAsync(Group group);
        Task EditGroupAsync(Group group);
        Task DeleteGroupAsync(string groupId);
        Task AddEmployeesToGroupAsync(IList<string> employeeIds, string groupId);
        Task AddGroupsToEmployeeAsync(IList<string> groupIds, string employeeId);
        Task<bool> CheckGroupNameAsync(string name);
    }
}