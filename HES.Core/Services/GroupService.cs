using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<Group> GetGroupByIdAsync(string groupId)
        {
            return await _groupRepository.GetByIdAsync(groupId);
        }

        public async Task<List<Group>> GetGroupsAsync()
        {
            return await _groupRepository
                .Query()
                .OrderBy(x => x.Name)
                .ToListAsync();
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
    }
}
