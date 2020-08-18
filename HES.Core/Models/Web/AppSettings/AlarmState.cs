using System;

namespace HES.Core.Models.Web.AppSettings
{
    public class AlarmState
    {
        public bool IsAlarm { get; set; }
        public string AdminName { get; set; }
        public DateTime DateTime { get; set; }
    }
}
