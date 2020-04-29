using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public class DeleteModel : PageModel
    {
        private readonly ILicenseService _licenseService;
        public IList<HardwareVaultLicense> DeviceLicenses { get; set; }

        [BindProperty]
        public LicenseOrder LicenseOrder { get; set; }


        public DeleteModel(ILicenseService licenseService)
        {
            _licenseService = licenseService;
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

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            await _licenseService.DeleteOrderAsync(id);

            return RedirectToPage("./Index");
        }
    }
}