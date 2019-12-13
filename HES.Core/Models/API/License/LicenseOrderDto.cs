using System;
using System.Collections.Generic;

namespace HES.Core.Models.API.License
{
    public class LicenseOrderDto
    {
        public string Id { get; set; }
        public string ContactEmail { get; set; }
        public List<string> Devices { get; set; }
        public string CustomerNote { get; set; }
        public DateTime? LicenseStartDate { get; set; }
        public DateTime? LicenseEndDate { get; set; }
        public bool ProlongExistingLicenses { get; set; }
        public string CustomerId { get; set; } = "BBB26599-81B8-44D5-80C0-31CF830F1578";
    }
}