using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
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

        public async Task<List<Group>> GetAllGroupsAsync(int skip, int take, string sortColumn, ListSortDirection sortDirection, string searchText)
        {
            searchText = searchText.ToLower().Trim();

            var query = _groupRepository.Query()
                            .Include(x => x.GroupMemberships)
                            .Where(x => x.Name.ToLower().Contains(searchText) ||
                                        x.Email.ToLower().Contains(searchText) ||
                                        x.GroupMemberships.Count.ToString().Contains(searchText))
                            .AsQueryable();

            switch (sortColumn)
            {
                case nameof(Group.Name):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
                    break;
                case nameof(Group.Email):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Email) : query.OrderByDescending(x => x.Email);
                    break;
                case nameof(Group.GroupMemberships):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.GroupMemberships.Count) : query.OrderByDescending(x => x.GroupMemberships.Count);
                    break;
            }

            return await query.Skip(skip).Take(take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetCountAsync(string searchText)
        {
            return await _groupRepository
                 .Query()
                 .CountAsync(x => x.Name.ToLower().Contains(searchText.ToLower().Trim()) ||
                                  x.Email.ToLower().Contains(searchText.ToLower().Trim()) ||
                                  x.GroupMemberships.Count().ToString().Contains(searchText.ToLower().Trim()));
        }

        public async Task<Group> GetGroupByIdAsync(string groupId)
        {
            return await _groupRepository.GetByIdAsync(groupId);
        }

        public async Task<Group> GetGroupByNameAsync(Group group)
        {
            return await _groupRepository.Query().FirstOrDefaultAsync(x => x.Name == group.Name);
        }

        public async Task<Group> CreateGroupAsync(Group group)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            var exist = await _groupRepository.ExistAsync(x => x.Name == group.Name);

            if (exist)
            {
                throw new AlreadyExistException("This name is already in use.");
            }

            return await _groupRepository.AddAsync(group);
        }

        public async Task EditGroupAsync(Group group)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            var exist = await _groupRepository.ExistAsync(x => x.Name == group.Name && x.Id != group.Id);

            if (exist)
            {
                throw new AlreadyExistException("This name is already in use.");
            }

            await _groupRepository.UpdateAsync(group);
        }

        public Task UnchangedGroupAsync(Group group)
        {
            return _groupRepository.Unchanged(group);
        }

        public async Task<Group> DeleteGroupAsync(string groupId)
        {
            if (groupId == null)
            {
                throw new ArgumentNullException(nameof(groupId));
            }

            var group = await GetGroupByIdAsync(groupId);

            if (group == null)
            {
                throw new NotFoundException("Group not found.");
            }

            return await _groupRepository.DeleteAsync(group);
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

        public async Task<List<Employee>> GetEmployeesSkipExistingInGroupAsync(string groupId)
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
                    var employeeExist = await _groupMembershipRepository.ExistAsync(x => x.EmployeeId == employeeId && x.GroupId == groupId);
                    if (employeeExist)
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


            foreach (var groupId in groupIds)
            {
                var employeeExist = await _groupMembershipRepository.ExistAsync(x => x.EmployeeId == employeeId && x.GroupId == groupId);
                if (employeeExist)
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
        }

        public async Task<GroupMembership> RemoveEmployeeFromGroupAsync(string groupMembershipId)
        {
            if (groupMembershipId == null)
            {
                throw new ArgumentNullException(nameof(groupMembershipId));
            }

            var groupMembership = await _groupMembershipRepository.GetByIdAsync(groupMembershipId);
            if (groupMembership == null)
            {
                throw new NotFoundException("GroupMembership not found.");
            }

            return await _groupMembershipRepository.DeleteAsync(groupMembership);
        }

        public async Task CreateGroupRangeAsync(List<Group> groups)
        {
            foreach (var group in groups)
            {
                try
                {
                    await CreateGroupAsync(group);
                }
                catch (AlreadyExistException)
                {
                    // Continue, if group exist
                }
            }
        }
    }
}
