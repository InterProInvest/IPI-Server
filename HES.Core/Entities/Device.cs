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
        public string Id { get; set; }
        public string MAC { get; set; }
        public string Model { get; set; }
        public string RFID { get; set; }
        public int Battery { get; set; }
        public string Firmware { get; set; }
        public DeviceState State { get; set; }
        public DateTime? LastSynced { get; set; }
        public string EmployeeId { get; set; }
        public string PrimaryAccountId { get; set; }
        public string AcceessProfileId { get; set; }
        public string MasterPassword { get; set; }
        public DateTime ImportedAt { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }
        [Display(Name = "Access Profile")]
        [ForeignKey("AcceessProfileId")]
        public DeviceAccessProfile DeviceAccessProfile { get; set; }

        [NotMapped]
        public bool IsOnline => RemoteDeviceConnectionsService.IsDeviceConnectedToHost(Id);
    }

    public enum DeviceState
    {
        OK,
        Locked,
        PendingUnlock,
        Disabled,
        Error
    }
}