using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HES.Web.Pages.HardwareVaults
{
    public class IndexModel : PageModel
    {
        public string DashboardFilter { get; set; }

        public void OnGet()
        {
        }

        public void OnGetLowBattery()
        {
            DashboardFilter = "LowBattery";
        }

        public void OnGetVaultLocked()
        {
            DashboardFilter = "VaultLocked";
        }

        public void OnGetVaultReady()
        {
            DashboardFilter = "VaultReady";
        }

        public void OnGetLicenseWarning()
        {
            DashboardFilter = "LicenseWarning";
        }

        public void OnGetLicenseCritical()
        {
            DashboardFilter = "LicenseCritical";
        }

        public void OnGetLicenseExpired()
        {
            DashboardFilter = "LicenseExpired";
        }
    }
}