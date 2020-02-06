namespace HES.Core.Models.API.Device
{
    public class DeviceImportDto
    {
        public string DeviceId { get; set; }
        public string MAC { get; set; }
        public string Model { get; set; }
        public string RFID { get; set; }
        public string Firmware { get; set; }
    }
}