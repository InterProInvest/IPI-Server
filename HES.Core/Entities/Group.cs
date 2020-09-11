using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class Group
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        /// <summary>
        /// Is Unique <see cref="ApplicationDbContext"/> 
        /// </summary>
        [Required]
        public string Name { get; set; }
        [RegularExpression(@"^[a-z0-9][-a-z0-9.!#$%&'*+-=?^_`{|}~\/]+@([-a-z0-9]+\.)+[a-z]{2,5}$", ErrorMessage = "The Email field is not a valid e-mail address.")]
        public string Email { get; set; }
        public string Description { get; set; }
        public bool ChangePasswordWhenExpired { get; set; }
        public List<GroupMembership> GroupMemberships { get; set; }
    }
}