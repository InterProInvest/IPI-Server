using HES.Core.Models.ActiveDirectory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ILdapService
    {
        Task<List<ActiveDirectoryUser>> GetUsersAsync(ActiveDirectoryCredential credentials);
        Task AddUsersAsync(List<ActiveDirectoryUser> users, bool createGroups);
        Task SetUserPasswordAsync(string employeeGuid, string password, ActiveDirectoryCredential credentials);
        Task<List<ActiveDirectoryGroup>> GetGroupsAsync(ActiveDirectoryCredential credentials);
        Task AddGroupsAsync(List<ActiveDirectoryGroup> groups, bool createEmployees);
    }
}