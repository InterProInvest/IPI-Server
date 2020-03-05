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

        public async Task AddAdUsersAsync(List<ActiveDirectoryUser> users, bool createGroups)
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

                if (createGroups && user.Groups != null)
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

        public List<ActiveDirectoryGroup> GetAdGroups(string server, string userName, string password)
        {
            var groups = new List<ActiveDirectoryGroup>();

            using (var context = new PrincipalContext(ContextType.Domain, server, userName, password))
            {
                GroupPrincipal group = new GroupPrincipal(context);
                PrincipalSearcher search = new PrincipalSearcher(group);

                foreach (var found in search.FindAll())
                {
                    GroupPrincipal groupPrincipal = found as GroupPrincipal;

                    if (groupPrincipal != null)
                    {
                        var activeDirectoryGroup = new ActiveDirectoryGroup()
                        {
                            Group = new Group()
                            {
                                Id = groupPrincipal.Guid.ToString(),
                                Name = groupPrincipal.Name,
                                Description = groupPrincipal.Description
                            }
                        };

                        List<Employee> employees = new List<Employee>();
                        foreach (var member in groupPrincipal.GetMembers())
                        {
                            UserPrincipal user = UserPrincipal.FindByIdentity(context, member.Name);

                            if (user != null && user.GivenName != null)
                            {
                                employees.Add(new Employee()
                                {
                                    Id = user.Guid.ToString(),
                                    FirstName = user.GivenName,
                                    LastName = user.Surname,
                                    Email = user.EmailAddress
                                });
                            }
                        }
                        activeDirectoryGroup.Employees = employees.Count > 0 ? employees : null;

                        groups.Add(activeDirectoryGroup);
                    }
                }
            }
            return groups;
        }

        public async Task AddAdGroupsAsync(List<ActiveDirectoryGroup> groups, bool createEmployees)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var group in groups)
                {
                    try
                    {
                        await _groupService.CreateGroupAsync(group.Group);
                    }
                    catch (Exception ex)
                    {
                        // TODO if group exist
                    }

                    if (createEmployees && group.Employees != null)
                    {
                        foreach (var employee in group.Employees)
                        {
                            try
                            {
                                await _employeeService.CreateEmployeeAsync(employee);
                            }
                            catch (Exception ex)
                            {
                                // TODO if exist
                            }
                        }
                        await _groupService.AddEmployeesToGroupAsync(group.Employees.Select(s => s.Id).ToList(), group.Group.Id);
                    }
                }

                transactionScope.Complete();
            }
        }
    }
}