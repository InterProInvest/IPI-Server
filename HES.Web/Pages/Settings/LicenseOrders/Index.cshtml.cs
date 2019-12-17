using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public class IndexModel : PageModel
    {
        private readonly ILicenseService _licenseService;
        public IList<LicenseOrder> LicenseOrder { get; set; }

        public IndexModel(ILicenseService licenseService)
        {
            _licenseService = licenseService;
        }

        public async Task OnGetAsync()
        {
            LicenseOrder = await _licenseService.GetLicenseOrdersAsync();
        }
    }
}