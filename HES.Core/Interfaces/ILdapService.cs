using HES.Core.Entities;
using System.Collections.Generic;

namespace HES.Core.Interfaces
{
    public interface ILdapService
    {
        List<Group> GetAdGroups(string server, string userName, string password);
    }
}