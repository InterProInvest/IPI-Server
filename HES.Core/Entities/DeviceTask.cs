using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class DeviceTask
    {
        [Key]
        public string Id { get; set; }
        public string OldName { get; set; }
        public string OldUrls { get; set; }
        public string OldApps { get; set; }
        public string OldLogin { get; set; }
        public string Password { get; set; }
        public string OtpSecret { get; set; }
        public TaskOperation Operation { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DeviceId { get; set; }
        public string DeviceAccountId { get; set; }

        [ForeignKey("DeviceAccountId")]
        public DeviceAccount DeviceAccount { get; set; }
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
        UnlockPin
    }
}