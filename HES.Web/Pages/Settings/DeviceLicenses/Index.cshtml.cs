using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web
{
    public class IndexModel : PageModel
    {
        private readonly IDeviceLicenseService _deviceLicenseService;
        public IList<DeviceLicense> DeviceLicense { get; set; }

        public IndexModel(IDeviceLicenseService deviceLicenseService)
        {
            _deviceLicenseService = deviceLicenseService;
        }

        public async Task OnGetAsync()
        {
            DeviceLicense = await _deviceLicenseService.GeDeviceLicensesAsync();
        }
    }
}
