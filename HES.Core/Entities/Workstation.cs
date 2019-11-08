using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HES.Core.Services;

namespace HES.Core.Entities
{
    public class Workstation
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
        [Display(Name = "Client Version")]
        public string ClientVersion { get; set; }
        public string DepartmentId { get; set; }
        public string OS { get; set; }
        public string IP { get; set; }
        [Display(Name = "Last Seen")]
        public DateTime LastSeen { get; set; }
        public bool Approved { get; set; }
        public bool RFID { get; set; }
        [Display(Name = "Devices")]
        public List<ProximityDevice> ProximityDevices { get; set; }

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        [NotMapped]
        public bool IsOnline => Id != null ? RemoteWorkstationConnectionsService.IsWorkstationConnectedToServer(Id) : false;
    }
}