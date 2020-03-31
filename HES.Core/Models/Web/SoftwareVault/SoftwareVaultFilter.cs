using HES.Core.Enums;

namespace HES.Core.Models.Web.SoftwareVault
{
    public class SoftwareVaultFilter
    {
        public string OS { get; set; }
        public string Model { get; set; }
        public string ClientAppVersion { get; set; }
        public VaultStatus? Status { get; set; }
        public LicenseStatus? LicenseStatus { get; set; }
        public string EmployeeId { get; set; }
        public string CompanyId { get; set; }
        public string DepartmentId { get; set; }
    }
}