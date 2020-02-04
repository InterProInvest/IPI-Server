using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class ProximityDevice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Display(Name = "Device")]
        public string DeviceId { get; set; }
        public string WorkstationId { get; set; }
        public int LockProximity { get; set; }
        public int UnlockProximity { get; set; }
        public int LockTimeout { get; set; }

        [ForeignKey("DeviceId")]
        public Device Device { get; set; }
        [ForeignKey("WorkstationId")]
        public Workstation Workstation { get; set; }
    }
}