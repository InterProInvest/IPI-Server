using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.API.License;
using HES.Core.Models.Web.License;
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
        private readonly IHardwareVaultService _deviceService;
        private readonly ILogger<CreateModel> _logger;

        public NewLicenseOrder NewLicenseOrderDto { get; set; }
        public RenewLicenseOrder RenewLicenseOrderDto { get; set; }
        public List<HardwareVault> NonLicensedDevices { get; set; }
        public List<HardwareVault> LicensedDevices { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public CreateModel(ILicenseService licenseService,
                           IHardwareVaultService deviceService,
                           ILogger<CreateModel> logger)
        {
            _licenseService = licenseService;
            _deviceService = deviceService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            NonLicensedDevices = await _deviceService
                .VaultQuery()
                .Where(d => d.LicenseStatus == VaultLicenseStatus.None || d.LicenseStatus == VaultLicenseStatus.Expired)
                .AsNoTracking()
                .ToListAsync();

            LicensedDevices = await _deviceService
                .VaultQuery()
                .Where(d => d.LicenseStatus != VaultLicenseStatus.None)
                .Where(d => d.LicenseStatus != VaultLicenseStatus.Expired)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateNewLicenseAsync(NewLicenseOrder newLicenseOrderDto, List<string> nonLicensedDevicesIds)
        {
            if (nonLicensedDevicesIds.Count == 0)
            {
                ErrorMessage = "No devices selected, select devices.";
                return RedirectToPage("./Create");
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = Validation.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Create");
            }

            try
            {
                var licenseOrder = new LicenseOrder()
                {
                    ContactEmail = newLicenseOrderDto.ContactEmail,
                    Note = newLicenseOrderDto.Note,
                    ProlongExistingLicenses = false,
                    StartDate = newLicenseOrderDto.StartDate,
                    EndDate = newLicenseOrderDto.EndDate
                };
                var createdOrder = await _licenseService.CreateOrderAsync(licenseOrder);
                await _licenseService.AddHardwareVaultLicensesAsync(createdOrder.Id, nonLicensedDevicesIds);
                SuccessMessage = "Order created";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostCreateRenewLicenseAsync(RenewLicenseOrder renewLicenseOrderDto, List<string> licensedDevicesIds)
        {
            if (licensedDevicesIds.Count == 0)
            {
                ErrorMessage = "No devices selected, select devices.";
                return RedirectToPage("./Create");
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = Validation.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Create");
            }

            try
            {
                var devices = await _deviceService.VaultQuery().Where(x => licensedDevicesIds.Contains(x.Id)).ToListAsync();
                var maxEndDateOfDevices = devices.Select(s => s.LicenseEndDate).Max();
                if (renewLicenseOrderDto.EndDate <= maxEndDateOfDevices)
                {
                    ErrorMessage = "The selected End Date less than current end date for selected devices";
                    return RedirectToPage("./Create");
                }

                var licenseOrder = new LicenseOrder()
                {
                    ContactEmail = renewLicenseOrderDto.ContactEmail,
                    Note = renewLicenseOrderDto.Note,
                    ProlongExistingLicenses = true,
                    StartDate = null,
                    EndDate = renewLicenseOrderDto.EndDate
                };
                var createdOrder = await _licenseService.CreateOrderAsync(licenseOrder);
                await _licenseService.AddHardwareVaultLicensesAsync(createdOrder.Id, licensedDevicesIds);
                SuccessMessage = "Order created";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Index");
        }
    }
}