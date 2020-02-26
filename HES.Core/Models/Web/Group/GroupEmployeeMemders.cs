using HES.Core.Entities;

namespace HES.Core.Models.Web.Group
{
    public class GroupEmployee
    {
        public Employee Employee { get; set; }
        public bool InGroup { get; set; }
    }
}