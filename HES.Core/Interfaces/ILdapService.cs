using HES.Core.Models.ActiveDirectory;
using HES.Core.Models.Web.AppSettings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ILdapService : IDisposable
    {
        Task<List<ActiveDirectoryUser>> GetUsersAsync(LdapSettings ldapSettings);
        Task AddUsersAsync(List<ActiveDirectoryUser> users, bool createGroups);
        Task SetUserPasswordAsync(string employeeId, string password, LdapSettings ldapSettings);
        Task<List<ActiveDirectoryGroup>> GetGroupsAsync(LdapSettings ldapSettings);
        Task ChangePasswordWhenExpiredAsync(string employeeId, LdapSettings ldapSettings);
        Task AddGroupsAsync(List<ActiveDirectoryGroup> groups, bool createEmployees);
    }
}