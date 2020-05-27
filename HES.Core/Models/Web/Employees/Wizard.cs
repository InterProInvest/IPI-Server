using HES.Core.Attributes;

namespace HES.Core.Models
{
    public class Wizard
    {
        [RequiredIf("SkipDevice")]
        public string DeviceId { get; set; }
        public bool SkipDevice { get; set; }

        [RequiredIf("SkipProximityUnlock")]
        public string WorkstationId { get; set; }
        public bool SkipProximityUnlock { get; set; }

        public WorkstationAccount WorkstationAccount { get; set; }
    }
}