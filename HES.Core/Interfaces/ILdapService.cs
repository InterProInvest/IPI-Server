using HES.Core.Entities;
using HES.Core.Models.ActiveDirectory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ILdapService
    {
        List<ActiveDirectoryUser> GetAdUsers(string server, string userName, string password);
        Task AddAdUsersAsync(List<ActiveDirectoryUser> users);
        List<Group> GetAdGroups(string server, string userName, string password);
    }
}