using HES.Core.Enums;
using HES.Core.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class Device
    {
        [Display(Name = "ID")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string MAC { get; set; }
        public string Model { get; set; }
        [Required]
        public string RFID { get; set; }
        public int Battery { get; set; }
        public string Firmware { get; set; }
        public DeviceState State { get; set; }
        public DateTime? LastSynced { get; set; }
        public string EmployeeId { get; set; }
        public string PrimaryAccountId { get; set; }
        public string AcceessProfileId { get; set; } = "default";
        public string MasterPassword { get; set; }
        public DateTime ImportedAt { get; set; }
        public bool HasNewLicense { get; set; }
        public LicenseStatus LicenseStatus { get; set; }
        public DateTime? LicenseEndDate { get; set; }
        
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        [Display(Name = "Access Profile")]
        [ForeignKey("AcceessProfileId")]
        public DeviceAccessProfile DeviceAccessProfile { get; set; }

        [NotMapped]
        public bool IsOnline => RemoteDeviceConnectionsService.IsDeviceConnectedToHost(Id);
    }    
}
