using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.API.License;
using HES.Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public class CreateModel : PageModel
    {
        private readonly ILicenseService _licenseService;
        private readonly IDeviceService _deviceService;
        private readonly ILogger<CreateModel> _logger;
        public LicenseOrder LicenseOrder { get; set; }
        public LicenseOrderDto LicenseOrderDto { get; set; }
        public List<Device> NonLicensedDevices { get; set; }
        public List<Device> LicensedDevices { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public CreateModel(ILicenseService licenseService,
                           IDeviceService deviceService,
                           ILogger<CreateModel> logger)
        {
            _licenseService = licenseService;
            _deviceService = deviceService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            NonLicensedDevices = await _deviceService
                .DeviceQuery()
                .Where(d => d.LicenseStatus == LicenseStatus.None)
                .AsNoTracking()
                .ToListAsync();

            LicensedDevices = await _deviceService
                .DeviceQuery()
                .Where(d => d.LicenseStatus != LicenseStatus.None)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateNewLicenseAsync(LicenseOrder licenseOrder, List<string> nonLicensedDevicesIds)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = ValidationHepler.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Create");
            }

            try
            {
                var createdOrder = await _licenseService.CreateOrderAsync(licenseOrder);
                await _licenseService.AddDeviceLicenseAsync(createdOrder.Id, nonLicensedDevicesIds);
                SuccessMessage = "Order created";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostCreateRenewLicenseAsync(LicenseOrder licenseOrder, List<string> licensedDevicesIds)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = ValidationHepler.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Create");
            }

            try
            {
                var createdOrder = await _licenseService.CreateOrderAsync(licenseOrder);
                await _licenseService.AddDeviceLicenseAsync(createdOrder.Id, licensedDevicesIds);
                SuccessMessage = "Order created";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Index");
        }


        //var licenseOrderDto = new LicenseOrderDto()
        //{
        //    Id = createdOrder.Id,
        //    ContactEmail = createdOrder.ContactEmail,
        //    CustomerNote = createdOrder.Note,
        //    LicenseStartDate = createdOrder.StartDate,
        //    LicenseEndDate = createdOrder.EndDate,
        //    ProlongExistingLicenses = createdOrder.ProlongExistingLicenses,
        //    CustomerId

        //}
    }
}