using HES.Core.Entities;
using HES.Core.Models.Web.Account;
using System.Collections.Generic;

namespace HES.Core.Models.ActiveDirectory
{
    public class ActiveDirectoryUser
    {
        public Employee Employee { get; set; }
        public WorkstationDomain DomainAccount { get; set; }
        public List<Group> Groups { get; set; }
        public bool Checked { get; set; }
    }
}