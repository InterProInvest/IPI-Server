namespace HES.Core.Entities
{
    public class GroupMembership
    {
        /// <summary>
        /// GroupId and EmployeeId is composite key <see cref="ApplicationDbContext"/> 
        /// </summary>
        public string GroupId { get; set; }
        public string EmployeeId { get; set; }
    }
}