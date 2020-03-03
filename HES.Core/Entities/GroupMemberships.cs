using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class GroupMembership
    {
        /// <summary>
        /// GroupId and EmployeeId is composite key <see cref="ApplicationDbContext"/> 
        /// </summary>
        [ForeignKey("Groups")]
        public string GroupId { get; set; }

        [ForeignKey("Employees")]
        public string EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }
    }
}