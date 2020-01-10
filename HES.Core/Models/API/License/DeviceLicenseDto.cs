using System;

namespace HES.Core.Models.API.License
{
    public class DeviceLicenseDto
    {
        public string DeviceId { get; set; }
        public DateTime LicenseEndDate { get; set; }
        public string Data { get; set; }
    }
}