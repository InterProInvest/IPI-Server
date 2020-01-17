using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public class DetailsModel : PageModel
    {
        private readonly ILicenseService _licenseService;
        private readonly ILogger<DetailsModel> _logger;
        public LicenseOrder LicenseOrder { get; set; }
        public IList<DeviceLicense> DeviceLicenses { get; set; }

        public DetailsModel(ILicenseService licenseService,
                            ILogger<DetailsModel> logger)
        {
            _licenseService = licenseService;
            _logger = logger;
        }


        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            LicenseOrder = await _licenseService.GetLicenseOrderByIdAsync(id);

            if (LicenseOrder == null)
            {
                return NotFound();
            }

            DeviceLicenses = await _licenseService.GetDeviceLicensesByOrderIdAsync(id);
            return Page();
        }
    }
}
