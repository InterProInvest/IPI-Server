using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class GroupMembership
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [ForeignKey("Groups")]
        public string GroupId { get; set; }


        [ForeignKey("Employees")]
        public string EmployeeId { get; set; }


        [ForeignKey("GroupId")]
        public Group Group { get; set; }


        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }
    }
}