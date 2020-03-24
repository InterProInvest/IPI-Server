using System.Collections.Generic;

namespace HES.Core.Models.API.Group
{
    public class AddEmployeeToGroupsDto
    {
        public IList<string> GroupIds { get; set; }
        public string EmployeeId { get; set; }
    }
}