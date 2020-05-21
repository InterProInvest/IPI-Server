using System.Collections.Generic;

namespace HES.Core.Models.API.Group
{
    public class AddEmployeesToGroupDto
    {
        public IList<string> EmployeeIds { get; set; }
        public string GroupId { get; set; }
    }
}