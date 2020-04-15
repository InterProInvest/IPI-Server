using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.ActiveDirectory;
using LdapForNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using static LdapForNet.Native.Native;

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

        public async Task<List<ActiveDirectoryUser>> GetUsersAsync(ActiveDirectoryCredential credentials)
        {
            var users = new List<ActiveDirectoryUser>();

            using (var connection = new LdapConnection())
            {
                connection.Connect(credentials.Host);
                await connection.BindAsync(LdapAuthType.Simple, new LdapCredential() { UserName = @$"{GetFirstDnFromHost(credentials.Host)}\{credentials.UserName}", Password = credentials.Password });

                var dn = GetDnFromHost(credentials.Host);

                var filter = "(&(objectCategory=user)(givenName=*))";
                var response = (SearchResponse)connection.SendRequest(new SearchRequest(dn, filter, LdapSearchScope.LDAP_SCOPE_SUBTREE));

                foreach (var entity in response.Entries)
                {
                    var activeDirectoryUser = new ActiveDirectoryUser()
                    {
                        Employee = new Employee()
                        {
                            Id = GetAttributeGUID(entity),
                            FirstName = TryGetAttribute(entity, "givenName"),
                            LastName = TryGetAttribute(entity, "sn"),
                            Email = TryGetAttribute(entity, "mail"),
                            PhoneNumber = TryGetAttribute(entity, "telephoneNumber")
                        }
                    };

                    List<Group> groups = new List<Group>();
                    var groupNames = TryGetAttributeArray(entity, "memberOf");
                    if (groupNames != null)
                    {
                        foreach (var groupName in groupNames)
                        {
                            var name = GetNameFromDn(groupName);
                            var filterGroup = $"(&(objectCategory=group)(name={name}))";
                            var groupResponse = (SearchResponse)connection.SendRequest(new SearchRequest(dn, filterGroup, LdapSearchScope.LDAP_SCOPE_SUBTREE));

                            groups.Add(new Group()
                            {
                                Id = GetAttributeGUID(groupResponse.Entries.First()),
                                Name = TryGetAttribute(groupResponse.Entries.First(), "name"),
                                Description = TryGetAttribute(groupResponse.Entries.First(), "description"),
                                Email = TryGetAttribute(groupResponse.Entries.First(), "mail")
                            });
                        }
                    }
                    activeDirectoryUser.Groups = groups.Count > 0 ? groups : null;

                    users.Add(activeDirectoryUser);
                }
            }

            return users.OrderBy(x => x.Employee.FullName).ToList();
        }

        //public List<ActiveDirectoryUser> GetAdUsers(string server, string userName, string password)
        //{
        //    var users = new List<ActiveDirectoryUser>();

        //    using (var context = new PrincipalContext(ContextType.Domain, server, userName, password))
        //    {
        //        UserPrincipal user = new UserPrincipal(context);
        //        PrincipalSearcher search = new PrincipalSearcher(user);

        //        foreach (var found in search.FindAll())
        //        {
        //            UserPrincipal userPrincipal = found as UserPrincipal;

        //            if (userPrincipal != null && userPrincipal.GivenName != null)
        //            {
        //                var activeDirectoryUser = new ActiveDirectoryUser()
        //                {
        //                    Employee = new Employee()
        //                    {
        //                        Id = userPrincipal.Guid.ToString(),
        //                        FirstName = userPrincipal.GivenName,
        //                        LastName = userPrincipal.Surname,
        //                        Email = userPrincipal.EmailAddress
        //                    }
        //                };

        //                try
        //                {
        //                    activeDirectoryUser.Groups = userPrincipal.GetGroups(context)
        //                        .Select(s => new Group()
        //                        {
        //                            Id = s.Guid.ToString(),
        //                            Name = s.Name,
        //                            Description = s.Description
        //                        })
        //                        .ToList();
        //                }
        //                catch (PrincipalOperationException)
        //                {
        //                    // If dns is not configured to connect to a domain
        //                    // information about the domain could not be retrieved.
        //                }

        //                users.Add(activeDirectoryUser);
        //            }
        //        }
        //    }
        //    return users.OrderBy(x => x.Employee.FullName).ToList();
        //}

        public async Task AddUsersAsync(List<ActiveDirectoryUser> users, bool createGroups)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var user in users)
                {
                    Employee employee = null;
                    try
                    {
                        employee = await _employeeService.CreateEmployeeAsync(user.Employee);
                    }
                    catch (AlreadyExistException)
                    {
                        // If user exist
                        employee = await _employeeService.GetEmployeeByFullNameAsync(user.Employee);
                    }

                    if (createGroups && user.Groups != null)
                    {
                        await _groupService.CreateGroupRangeAsync(user.Groups);
                        await _groupService.AddEmployeeToGroupsAsync(employee.Id, user.Groups.Select(s => s.Id).ToList());
                    }
                }
                transactionScope.Complete();
            }
        }

        public async Task SetUserPasswordAsync(string employeeId, string password, ActiveDirectoryCredential credentials)
        {
            using (var connection = new LdapConnection())
            {
                connection.Connect(new Uri($"ldaps://{credentials.Host}:636"));
                connection.Bind(LdapAuthType.Simple, new LdapCredential() { UserName = @$"addc\{credentials.UserName}", Password = credentials.Password });

                var objectGUID = GetObjectGuid(employeeId);
                var user = (SearchResponse)connection.SendRequest(new SearchRequest("dc=addc,dc=hideez,dc=com", $"(&(objectCategory=user)(objectGUID={objectGUID}))", LdapSearchScope.LDAP_SCOPE_SUBTREE));

                await connection.ModifyAsync(new LdapModifyEntry
                {
                    Dn = user.Entries.First().Dn,
                    Attributes = new List<LdapModifyAttribute>
                        {
                            new LdapModifyAttribute
                            {
                                LdapModOperation = LdapModOperation.LDAP_MOD_REPLACE,
                                Type = "userPassword",
                                Values = new List<string> { password }
                            }
                        }
                });
            }

        }

        public async Task<List<ActiveDirectoryGroup>> GetGroupsAsync(ActiveDirectoryCredential credentials)
        {
            var groups = new List<ActiveDirectoryGroup>();

            using (var connection = new LdapConnection())
            {
                connection.Connect(credentials.Host);
                await connection.BindAsync(LdapAuthType.Simple, new LdapCredential() { UserName = @$"{GetFirstDnFromHost(credentials.Host)}\{credentials.UserName}", Password = credentials.Password });

                var dn = GetDnFromHost(credentials.Host);

                var filter = "(objectCategory=group)";
                var response = (SearchResponse)connection.SendRequest(new SearchRequest(dn, filter, LdapSearchScope.LDAP_SCOPE_SUBTREE));

                foreach (var entity in response.Entries)
                {
                    var activeDirectoryGroup = new ActiveDirectoryGroup()
                    {
                        Group = new Group()
                        {
                            Id = GetAttributeGUID(entity),
                            Name = TryGetAttribute(entity, "name"),
                            Description = TryGetAttribute(entity, "description"),
                            Email = TryGetAttribute(entity, "mail")
                        }
                    };

                    List<Employee> employees = new List<Employee>();
                    var filterMembers = $"(&(objectCategory=user)(memberOf={entity.Dn})(givenName=*))";
                    var members = (SearchResponse)connection.SendRequest(new SearchRequest(dn, filterMembers, LdapSearchScope.LDAP_SCOPE_SUBTREE));

                    foreach (var member in members.Entries)
                    {
                        employees.Add(new Employee()
                        {
                            Id = GetAttributeGUID(member),
                            FirstName = TryGetAttribute(member, "givenName"),
                            LastName = TryGetAttribute(member, "sn"),
                            Email = TryGetAttribute(member, "mail"),
                            PhoneNumber = TryGetAttribute(member, "telephoneNumber")
                        });
                    }

                    activeDirectoryGroup.Employees = employees.Count > 0 ? employees : null;
                    groups.Add(activeDirectoryGroup);
                }
            }

            return groups.OrderBy(x => x.Group.Name).ToList();
        }

        //public List<ActiveDirectoryGroup> GetAdGroups(string server, string userName, string password)
        //{
        //    var groups = new List<ActiveDirectoryGroup>();

        //    using (var context = new PrincipalContext(ContextType.Domain, server, userName, password))
        //    {
        //        GroupPrincipal group = new GroupPrincipal(context);
        //        PrincipalSearcher search = new PrincipalSearcher(group);

        //        foreach (var found in search.FindAll())
        //        {
        //            GroupPrincipal groupPrincipal = found as GroupPrincipal;

        //            if (groupPrincipal != null)
        //            {
        //                var activeDirectoryGroup = new ActiveDirectoryGroup()
        //                {
        //                    Group = new Group()
        //                    {
        //                        Id = groupPrincipal.Guid.ToString(),
        //                        Name = groupPrincipal.Name,
        //                        Description = groupPrincipal.Description
        //                    }
        //                };

        //                List<Employee> employees = new List<Employee>();
        //                try
        //                {
        //                    foreach (var member in groupPrincipal.GetMembers())
        //                    {
        //                        UserPrincipal user = UserPrincipal.FindByIdentity(context, member.Name);

        //                        if (user != null && user.GivenName != null)
        //                        {
        //                            employees.Add(new Employee()
        //                            {
        //                                Id = user.Guid.ToString(),
        //                                FirstName = user.GivenName,
        //                                LastName = user.Surname,
        //                                Email = user.EmailAddress
        //                            });
        //                        }
        //                    }
        //                }
        //                catch (PrincipalOperationException)
        //                {
        //                    // If dns is not configured to connect to a domain
        //                    // information about the domain could not be retrieved.
        //                }

        //                activeDirectoryGroup.Employees = employees.Count > 0 ? employees : null;

        //                groups.Add(activeDirectoryGroup);
        //            }
        //        }
        //    }
        //    return groups.OrderBy(x => x.Group.Name).ToList();
        //}

        public async Task AddGroupsAsync(List<ActiveDirectoryGroup> groups, bool createEmployees)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var group in groups)
                {
                    Group currentGroup = null;
                    try
                    {
                        currentGroup = await _groupService.CreateGroupAsync(group.Group);
                    }
                    catch (AlreadyExistException)
                    {
                        // If group exist
                        currentGroup = await _groupService.GetGroupByNameAsync(group.Group);
                    }

                    if (createEmployees && group.Employees != null)
                    {
                        foreach (var employee in group.Employees)
                        {
                            Employee currentEmployee = null;
                            try
                            {
                                currentEmployee = await _employeeService.CreateEmployeeAsync(employee);
                            }
                            catch (AlreadyExistException)
                            {
                                // If user exist
                                currentEmployee = await _employeeService.GetEmployeeByFullNameAsync(employee);
                                employee.Id = currentEmployee.Id;
                            }
                        }
                        await _groupService.AddEmployeesToGroupAsync(group.Employees.Select(s => s.Id).ToList(), currentGroup.Id);
                    }
                }
                transactionScope.Complete();
            }
        }

        #region Utils

        private string GetAttributeGUID(DirectoryEntry entry)
        {
            return new Guid(entry.Attributes["objectGUID"].GetValues<byte[]>().First()).ToString();
        }

        private string TryGetAttribute(DirectoryEntry entry, string attr)
        {
            DirectoryAttribute directoryAttribute;
            return entry.Attributes.TryGetValue(attr, out directoryAttribute) == true ? directoryAttribute.GetValues<string>().First() : null;
        }

        private string[] TryGetAttributeArray(DirectoryEntry entry, string attr)
        {
            DirectoryAttribute directoryAttribute;
            return entry.Attributes.TryGetValue(attr, out directoryAttribute) == true ? directoryAttribute.GetValues<string>().ToArray() : null;
        }

        private string GetDnFromHost(string hostname)
        {
            char separator = '.';
            var parts = hostname.Split(separator);
            var dnParts = parts.Select(_ => $"dc={_}");
            return string.Join(",", dnParts);
        }

        private string GetFirstDnFromHost(string hostname)
        {
            char separator = '.';
            var parts = hostname.Split(separator);
            return parts[0];
        }

        private string GetNameFromDn(string dn)
        {
            char separator = ',';
            var parts = dn.Split(separator);
            return parts[0].Replace("CN=", string.Empty);
        }

        private string GetObjectGuid(string guid)
        {
            var ba = new Guid(guid).ToByteArray();
            var hex = BitConverter.ToString(ba).Insert(0, @"\").Replace("-", @"\");
            return hex;
        }

        #endregion
    }
}