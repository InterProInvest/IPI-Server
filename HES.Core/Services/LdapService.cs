using HES.Core.Entities;
using HES.Core.Interfaces;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;

namespace HES.Core.Services
{
    public class LdapService : ILdapService
    {
        public List<Group> GetAdGroups(string server, string userName, string password)
        {
            var list = new List<Group>();

            using (var context = new PrincipalContext(ContextType.Domain, server, userName, password))
            {
                GroupPrincipal group = new GroupPrincipal(context);
                PrincipalSearcher search = new PrincipalSearcher(group);

                foreach (var found in search.FindAll())
                {
                    GroupPrincipal gp = found as GroupPrincipal;

                    if (gp != null)
                    {
                        var isUserGroup = gp.DistinguishedName.Contains("Builtin") ? false : true;
                        list.Add(new Group()
                        {
                            Id = gp.Guid.ToString(),
                            Name = gp.Name,
                            Description = gp.Description,
                            IsUserGroup = isUserGroup
                        });
                    }
                }
            }
            return list;
        }
    }
}
