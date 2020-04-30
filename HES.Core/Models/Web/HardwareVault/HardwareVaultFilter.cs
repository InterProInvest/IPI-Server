using System;
using HES.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.HardwareVault
{
    public class HardwareVaultFilter
    {
        public string Id { get; set; }
        public string MAC { get; set; }
        public string Model { get; set; }
        public string RFID { get; set; }
        [Range(0, 100)]
        public int? Battery { get; set; }
        public string Firmware { get; set; }
        public DateTime? LastSyncedStartDate { get; set; }
        public DateTime? LastSyncedEndDate { get; set; }
        public string Employee { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        public VaultStatus? VaultStatus { get; set; }
        public VaultLicenseStatus? LicenseStatus { get; set; }
        public DateTime? LicenseEndDate { get; set; }
    }
}