using HES.Core.Models.ActiveDirectory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ILdapService
    {
        Task<List<ActiveDirectoryUser>> GetUsersAsync(ActiveDirectoryCredential credentials);
        //List<ActiveDirectoryUser> GetAdUsers(string server, string userName, string password);
        Task AddUsersAsync(List<ActiveDirectoryUser> users, bool createGroups);
        Task<List<ActiveDirectoryGroup>> GetGroupsAsync(ActiveDirectoryCredential credentials);
        //List<ActiveDirectoryGroup> GetAdGroups(string server, string userName, string password);
        Task AddGroupsAsync(List<ActiveDirectoryGroup> groups, bool createEmployees);
    }
}