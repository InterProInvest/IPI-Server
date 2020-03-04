using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Core.Services
{
    public class LdapService : ILdapService
    {
        private readonly IEmployeeService _employeeService;
        private readonly IGroupService _groupService;

        public LdapService(IEmployeeService employeeService, IGroupService groupService)
        {
            _employeeService = employeeService;
            _groupService = groupService;
        }

        public List<ActiveDirectoryUser> GetAdUsers(string server, string userName, string password)
        {
            var users = new List<ActiveDirectoryUser>();

            using (var context = new PrincipalContext(ContextType.Domain, server, userName, password))
            {
                UserPrincipal user = new UserPrincipal(context);
                PrincipalSearcher search = new PrincipalSearcher(user);

                foreach (var found in search.FindAll())
                {
                    UserPrincipal userPrincipal = found as UserPrincipal;

                    if (userPrincipal != null && userPrincipal.GivenName != null)
                    {
                        var activeDirectoryUser = new ActiveDirectoryUser()
                        {
                            Employee = new Employee()
                            {
                                Id = userPrincipal.Guid.ToString(),
                                FirstName = userPrincipal.GivenName,
                                LastName = userPrincipal.Surname,
                                Email = userPrincipal.EmailAddress
                            }
                        };

                        try
                        {
                            activeDirectoryUser.Groups = userPrincipal.GetGroups(context)
                                .Select(s => new Group()
                                {
                                    Id = s.Guid.ToString(),
                                    Name = s.Name,
                                    Description = s.Description
                                })
                                .ToList();
                        }
                        catch (PrincipalOperationException)
                        {
                            // If dns is not configured to connect to a domain
                            // information about the domain could not be retrieved.
                        }

                        users.Add(activeDirectoryUser);
                    }
                }
            }
            return users;
        }

        public async Task AddAdUsersAsync(List<ActiveDirectoryUser> users)
        {
            foreach (var user in users)
            {
                try
                {
                    await _employeeService.CreateEmployeeAsync(user.Employee);
                }
                catch (Exception)
                {
                    // TODO if user exist
                }

                if (user.Groups != null)
                {
                    using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        await _groupService.CreateGroupRangeAsync(user.Groups);
                        await _groupService.AddEmployeeToGroupsAsync(user.Employee.Id, user.Groups.Select(s => s.Id).ToList());
                        transactionScope.Complete();
                    }
                }
            }
        }

        public List<Group> GetAdGroups(string server, string userName, string password)
        {
            var list = new List<Group>();

            using (var context = new PrincipalContext(ContextType.Domain, server, userName, password))
            {
                GroupPrincipal group = new GroupPrincipal(context);
                PrincipalSearcher search = new PrincipalSearcher(group);

                foreach (var found in search.FindAll())
                {
                    GroupPrincipal groupPrincipal = found as GroupPrincipal;

                    if (groupPrincipal != null)
                    {
                        var isUserGroup = groupPrincipal.DistinguishedName.Contains("Builtin") ? false : true;
                        list.Add(new Group()
                        {
                            Id = groupPrincipal.Guid.ToString(),
                            Name = groupPrincipal.Name,
                            Description = groupPrincipal.Description,
                            IsUserGroup = isUserGroup
                        });
                    }
                }
            }
            return list;
        }
    }
}