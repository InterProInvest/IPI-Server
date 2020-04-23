using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class DeviceTask
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string Password { get; set; }
        public string OtpSecret { get; set; }
        public TaskOperation Operation { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DeviceId { get; set; }
        public string AccountId { get; set; }

        [ForeignKey("AccountId")]
        public Account Account { get; set; }
    }

    public enum TaskOperation
    {
        None,
        Create,
        Update,
        Delete,
        Wipe,
        Link,
        Primary,
        Profile,
        UnlockPin,
        Lock
    }
}