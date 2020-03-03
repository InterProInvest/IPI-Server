using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Group;
using HES.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Core.Services
{
    public class GroupService : IGroupService
    {
        private readonly IAsyncRepository<Group> _groupRepository;
        private readonly IAsyncRepository<GroupMembership> _groupMembershipRepository;
        private readonly IAsyncRepository<Employee> _employeeRepository;

        public GroupService(IAsyncRepository<Group> groupRepository,
                            IAsyncRepository<GroupMembership> groupMembershipRepository,
                            IAsyncRepository<Employee> employeeRepository)
        {
            _groupRepository = groupRepository;
            _groupMembershipRepository = groupMembershipRepository;
            _employeeRepository = employeeRepository;
        }

        public IQueryable<Group> Query()
        {
            return _groupRepository.Query();
        }

        public async Task<IList<Group>> GetAllGroupsAsync(int skip, int take, ListSortDirection sortDirection = ListSortDirection.Descending, string search = null, string orderBy = nameof(Group.Name))
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return await _groupRepository.Query()
                    .Include(x => x.GroupMemberships)
                    .OrderByDynamic(orderBy, sortDirection == ListSortDirection.Ascending ? false : true)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
            }

            search = search.ToLower().Trim();

            return await _groupRepository.Query()
                    .Include(x => x.GroupMemberships)
                    .Where(x => x.Name.ToLower().Contains(search) ||
                        x.Email.ToLower().Contains(search) ||
                        x.GroupMemberships.Count.ToString().Contains(search))
                    .OrderByDynamic(orderBy, sortDirection == ListSortDirection.Ascending ? false : true)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
        }

        public async Task<int> GetCountAsync(string search = null)
        {
            return await _groupRepository.Query().SearchCountAsync(search);
        }

        public async Task<Group> GetGroupByIdAsync(string groupId)
        {
            return await _groupRepository.GetByIdAsync(groupId);
        }

        public async Task<Group> CreateGroupAsync(Group group)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }
            return await _groupRepository.AddAsync(group);
        }

        public async Task EditGroupAsync(Group group)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            await _groupRepository.UpdateAsync(group);
        }

        public async Task DeleteGroupAsync(string groupId)
        {
            if (groupId == null)
            {
                throw new ArgumentNullException(nameof(groupId));
            }

            var group = await GetGroupByIdAsync(groupId);

            if (group == null)
            {
                throw new Exception("Group does not exist.");
            }

            await _groupRepository.DeleteAsync(group);
        }

        public async Task<List<GroupMembership>> GetGruopMembersAsync(string groupId)
        {
            return await _groupMembershipRepository
                .Query()
                .Include(x => x.Employee)
                .Where(x => x.GroupId == groupId)
                .ToListAsync();
        }

        public async Task<GroupMembership> GetGroupMembershipAsync(string employeeId, string groupId)
        {
            return await _groupMembershipRepository
                .Query()
                .Include(x => x.Employee)
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.EmployeeId == employeeId);
        }

        public async Task<List<Employee>> GetEmployeesSkipExistingAsync(string groupId)
        {
            var members = await GetGruopMembersAsync(groupId);

            return await _employeeRepository
                .Query()
                .Where(x => !members.Select(s => s.EmployeeId).Contains(x.Id))
                .ToListAsync();
        }

        public async Task AddEmployeesToGroupAsync(IList<string> employeeIds, string groupId)
        {
            if (employeeIds == null)
            {
                throw new ArgumentNullException(nameof(employeeIds));
            }

            if (groupId == null)
            {
                throw new ArgumentNullException(nameof(groupId));
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var employeeId in employeeIds)
                {
                    var groupMembership = new GroupMembership()
                    {
                        EmployeeId = employeeId,
                        GroupId = groupId
                    };

                    await _groupMembershipRepository.AddAsync(groupMembership);
                }
                transactionScope.Complete();
            }
        }

        public async Task AddEmployeeToGroupsAsync(string employeeId, IList<string> groupIds)
        {
            if (employeeId == null)
            {
                throw new ArgumentNullException(nameof(employeeId));
            }

            if (groupIds == null)
            {
                throw new ArgumentNullException(nameof(groupIds));
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var groupId in groupIds)
                {
                    var employeeExistInGroup = await _groupMembershipRepository.ExistAsync(x => x.EmployeeId == employeeId && x.GroupId == groupId);
                    if (employeeExistInGroup)
                    {
                        continue;
                    }

                    var groupMembership = new GroupMembership()
                    {
                        EmployeeId = employeeId,
                        GroupId = groupId
                    };

                    await _groupMembershipRepository.AddAsync(groupMembership);
                }
                transactionScope.Complete();
            }
        }

        public async Task RemoveEmployeeFromGroupAsync(string employeeId, string groupId)
        {
            if (employeeId == null)
            {
                throw new ArgumentNullException(nameof(employeeId));
            }

            if (groupId == null)
            {
                throw new ArgumentNullException(nameof(groupId));
            }

            var groupMembership = await _groupMembershipRepository.GetByCompositeKeyAsync(new object[] { groupId, employeeId });
            if (groupMembership == null)
            {
                throw new Exception("GroupMembership does not exist.");
            }

            await _groupMembershipRepository.DeleteAsync(groupMembership);
        }

        public async Task ManageEmployeesAsync(List<GroupEmployee> groupEmployees, string groupId)
        {
            if (groupEmployees == null)
            {
                throw new ArgumentNullException(nameof(groupEmployees));
            }

            if (groupId == null)
            {
                throw new ArgumentNullException(nameof(groupId));
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var member in groupEmployees)
                {
                    var groupMembership = await _groupMembershipRepository.GetByCompositeKeyAsync(new object[] { groupId, member.Employee.Id });

                    if (member.InGroup && groupMembership == null)
                    {
                        groupMembership = new GroupMembership() { GroupId = groupId, EmployeeId = member.Employee.Id };
                        await _groupMembershipRepository.AddAsync(groupMembership);
                        continue;
                    }

                    if (!member.InGroup && groupMembership != null)
                    {
                        await _groupMembershipRepository.DeleteAsync(groupMembership);
                    }
                }
                transactionScope.Complete();
            }
        }

        public async Task<bool> CheckGroupNameAsync(string name)
        {
            return await _groupRepository.Query().AnyAsync(x => x.Name == name);
        }

        public async Task CreateGroupRangeAsync(List<Group> groups)
        {
            foreach (var group in groups)
            {
                var exist = await _groupRepository.ExistAsync(x => x.Name == group.Name);
                if (exist)
                {
                    continue;
                }
                await CreateGroupAsync(group);
            }
        }
    }
}
