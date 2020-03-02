using HES.Core.Entities;
using System.Collections.Generic;

namespace HES.Core.Models.ActiveDirectory
{
    public class ActiveDirectoryUser
    {
        public Employee Employee { get; set; }
        public List<Group> Groups { get; set; }

    }
}