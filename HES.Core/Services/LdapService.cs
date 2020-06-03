using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.ActiveDirectory;
using HES.Core.Models.Web.Account;
using HES.Core.Models.Web.AppSettings;
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

        public async Task<List<ActiveDirectoryUser>> GetUsersAsync(LdapSettings ldapSettings)
        {
            var users = new List<ActiveDirectoryUser>();

            using (var connection = new LdapConnection())
            {
                connection.Connect(ldapSettings.Host, 389);
                await connection.BindAsync(LdapAuthType.Simple, CreateLdapCredential(ldapSettings));

                var dn = GetDnFromHost(ldapSettings.Host);

                var filter = "(&(objectCategory=user)(givenName=*))";
                var response = (SearchResponse)connection.SendRequest(new SearchRequest(dn, filter, LdapSearchScope.LDAP_SCOPE_SUBTREE));

                foreach (var entity in response.Entries)
                {
                    var activeDirectoryUser = new ActiveDirectoryUser()
                    {
                        Employee = new Employee()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ActiveDirectoryGuid = GetAttributeGUID(entity),
                            FirstName = TryGetAttribute(entity, "givenName"),
                            LastName = TryGetAttribute(entity, "sn"),
                            Email = TryGetAttribute(entity, "mail"),
                            PhoneNumber = TryGetAttribute(entity, "telephoneNumber")
                        },
                        DomainAccount = new WorkstationDomain()
                        {
                            Name = "Domain Account",
                            Domain = GetFirstDnFromHost(ldapSettings.Host),
                            UserName = TryGetAttribute(entity, "sAMAccountName")
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

        public async Task AddUsersAsync(List<ActiveDirectoryUser> users, bool createGroups)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var user in users)
                {
                    Employee employee = null;
                    employee = await _employeeService.ImportEmployeeAsync(user.Employee);

                    try
                    {
                        // The employee may already be in the database, so we get his ID and create an account
                        user.DomainAccount.EmployeeId = employee.Id;
                        await _employeeService.CreateWorkstationAccountAsync(user.DomainAccount);
                    }
                    catch (AlreadyExistException)
                    {
                        // Ignore if a domain account exists
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

        public async Task SetUserPasswordAsync(string employeeId, string password, LdapSettings ldapSettings)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);

            using (var connection = new LdapConnection())
            {
                connection.Connect(new Uri($"ldaps://{ldapSettings.Host}:636"));
                connection.Bind(LdapAuthType.Simple, CreateLdapCredential(ldapSettings));

                var objectGUID = GetObjectGuid(employee.ActiveDirectoryGuid);
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

        public async Task<List<ActiveDirectoryGroup>> GetGroupsAsync(LdapSettings ldapSettings)
        {
            var groups = new List<ActiveDirectoryGroup>();

            using (var connection = new LdapConnection())
            {
                connection.Connect(ldapSettings.Host, 389);
                await connection.BindAsync(LdapAuthType.Simple, CreateLdapCredential(ldapSettings));

                var dn = GetDnFromHost(ldapSettings.Host);

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
                            Id = Guid.NewGuid().ToString(),
                            ActiveDirectoryGuid = GetAttributeGUID(member),
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
                            var imported = await _employeeService.ImportEmployeeAsync(employee);
                            employee.Id = imported.Id;
                        }
                        await _groupService.AddEmployeesToGroupAsync(group.Employees.Select(s => s.Id).ToList(), currentGroup.Id);
                    }
                }
                transactionScope.Complete();
            }
        }

        #region Utils

        private LdapCredential CreateLdapCredential(LdapSettings ldapSettings)
        {
            return new LdapCredential() { UserName = @$"{GetFirstDnFromHost(ldapSettings.Host)}\{ldapSettings.UserName}", Password = ldapSettings.Password };
        }

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
