using HES.Core.Entities;
using System.Collections.Generic;

namespace HES.Core.Models.ActiveDirectory
{
    public class ActiveDirectoryGroup
    {
        public Group Group { get; set; }
        public List<Employee> Employees { get; set; }
    }
}