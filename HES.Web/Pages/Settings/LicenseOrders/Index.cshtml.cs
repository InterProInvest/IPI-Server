using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.API.License;
using HES.Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public class IndexModel : PageModel
    {
        private readonly ILicenseService _licenseService;
        private readonly ILogger<IndexModel> _logger;
        public IList<LicenseOrder> LicenseOrder { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(ILicenseService licenseService, ILogger<IndexModel> logger)
        {
            _licenseService = licenseService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            LicenseOrder = await _licenseService.GetLicenseOrdersAsync();
        }

        public async Task<IActionResult> OnPostSendOrderAsync(string orderId)
        {
            try
            {
                await _licenseService.SendOrderAsync(orderId);
                SuccessMessage = "License order has been sent.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }
    }
}