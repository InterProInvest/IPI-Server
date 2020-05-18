using HES.Core.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HES.Core.Models.API.HardwareVault
{
    public class ImportHardwareVaultDto
    {
        [JsonProperty("DevicesDto")]
        public List<HardwareVaultDto> HardwareVaultsDto { get; set; }

        [JsonProperty("DeviceLicensesDto")]
        public List<HardwareVaultLicenseDto> HardwareVaultLicensesDto { get; set; }

        [JsonProperty("LicenseOrdersDto")]
        public List<LicenseOrderDto> LicenseOrdersDto { get; set; }
    }

    public class HardwareVaultDto
    {
        [JsonProperty("DeviceId")]
        public string HardwareVaultId { get; set; }
        public string MAC { get; set; }
        public string Model { get; set; }
        public string RFID { get; set; }
        public string Firmware { get; set; }
    }

    public class HardwareVaultLicenseDto
    {
        public string Id { get; set; }
        public string LicenseOrderId { get; set; }

        [JsonProperty("DeviceId")]
        public string HardwareVaultId { get; set; }
        public DateTime EndDate { get; set; }
        public string Data { get; set; }
    } 
    
    public class LicenseOrderDto
    {
        public string Id { get; set; }
        public string ContactEmail { get; set; }
        public string Note { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool ProlongExistingLicenses { get; set; }
        public DateTime CreatedAt { get; set; }
        public LicenseOrderStatus OrderStatus { get; set; }
    }
}