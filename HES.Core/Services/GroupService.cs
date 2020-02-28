using System;
using System.Linq;
using HES.Core.Entities;
using HES.Core.Utilities;
using HES.Core.Interfaces;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HES.Core.Services
{
    public class GroupService : IGroupService
    {
        private readonly IAsyncRepository<Group> _groupRepository;

        public GroupService(IAsyncRepository<Group> groupRepository)
        {
            _groupRepository = groupRepository;
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
                    .OrderByDynamic(orderBy, sortDirection == ListSortDirection.Ascending ? false : true)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
            }

            search = search.ToLower().Trim();

            return await _groupRepository.Query()
                    .Where(x => x.Name.ToLower().Contains(search) || 
                        x.Email.ToLower().Contains(search) || 
                        x.Description.ToLower().Contains(search))
                    .OrderByDynamic(orderBy, sortDirection == ListSortDirection.Ascending ? false : true)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
        }

        public async Task<int> GetCountAsync(string search = null)
        {
            return await _groupRepository.Query().SearchCountAsync();
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

        public async Task AddEmployeesToGroupAsync(IList<string> employeeIds, string groupId)
        {
            throw new NotImplementedException();
        }

        public async Task AddGroupsToEmployeeAsync(IList<string> groupIds, string employeeId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CheckGroupNameAsync(string name)
        {
            return await _groupRepository.Query().AnyAsync(x => x.Name == name);
        }
    }
}