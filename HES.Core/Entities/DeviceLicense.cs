using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities
{
    public class DeviceLicense
    {
        [Key]
        public string Id { get; set; }
        public string LicenseOrderId { get; set; }
        public string DeviceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ImportedAt { get; set; }
        public DateTime LicenseEndDate { get; set; }
        public int Capabilities { get; set; }
        public DateTime? AppliedAt { get; set; }
        public byte[] Data { get; set; }
    }
}
