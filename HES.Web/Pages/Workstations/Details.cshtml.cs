using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public class DetailsModel : PageModel
    {
        private readonly IWorkstationService _workstationService;
        private readonly IHardwareVaultService _deviceService;
        private readonly ILogger<DetailsModel> _logger;

        public IList<WorkstationProximityVault> ProximityDevices { get; set; }
        public IList<HardwareVault> Devices { get; set; }
        public Workstation Workstation { get; set; }
        public WorkstationProximityVault ProximityDevice { get; set; }
        public bool WarningMessage { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public DetailsModel(IWorkstationService workstationService,
                            IHardwareVaultService deviceService,
                            ILogger<DetailsModel> logger)
        {
            _workstationService = workstationService;
            _deviceService = deviceService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Workstation = await _workstationService.GetWorkstationByIdAsync(id);

            if (Workstation == null)
            {
                _logger.LogWarning($"{nameof(Workstation)} is null");
                return NotFound();
            }

            ProximityDevices = await _workstationService.GetProximityVaultsByWorkstationIdAsync(id);

            if (ProximityDevices == null)
            {
                _logger.LogWarning($"{nameof(ProximityDevices)} is null");
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnGetAddProximityDeviceAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Workstation = await _workstationService.GetWorkstationByIdAsync(id);

            if (Workstation == null)
            {
                _logger.LogWarning($"{nameof(Workstation)} is null");
                return NotFound();
            }

            ProximityDevices = await _workstationService.GetProximityVaultsByWorkstationIdAsync(id);

            if (ProximityDevices.Count >= 1)
            {
                WarningMessage = true;
            }

            var deviceQuery = _deviceService.VaultQuery();

            foreach (var proximityDevice in ProximityDevices)
            {
                deviceQuery = deviceQuery.Where(d => d.Id != proximityDevice.HardwareVaultId);
            }

            Devices = await deviceQuery
                .Include(d => d.Employee)
                .ToListAsync();

            return Partial("_AddProximityDevice", this);
        }

        public async Task<IActionResult> OnPostAddProximityDeviceAsync(string workstationId, string[] devicesId)
        {
            if (workstationId == null)
            {
                _logger.LogWarning($"{nameof(workstationId)} is null");
                return NotFound();
            }

            try
            {
                await _workstationService.AddProximityVaultsAsync(workstationId, devicesId);
                await _workstationService.UpdateProximitySettingsAsync(workstationId);

                SuccessMessage = "Device(s) added.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            var id = workstationId;
            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnGetEditProximitySettingsAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            ProximityDevice = await _workstationService
                .ProximityVaultQuery()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ProximityDevice == null)
            {
                _logger.LogWarning($"{nameof(ProximityDevice)} is null");
                return NotFound();
            }

            return Partial("_EditProximitySettings", this);
        }

        public async Task<IActionResult> OnPostEditProximitySettingsAsync(WorkstationProximityVault proximityDevice)
        {
            var id = proximityDevice.WorkstationId;
            if (!ModelState.IsValid)
            {
                ErrorMessage = Validation.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Details", new { id });
            }

            try
            {
                await _workstationService.EditProximityVaultAsync(proximityDevice);
                await _workstationService.UpdateProximitySettingsAsync(proximityDevice.WorkstationId);

                SuccessMessage = $"Proximity settings updated.";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnGetDeleteProximityDeviceAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            ProximityDevice = await _workstationService
                .ProximityVaultQuery()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ProximityDevice == null)
            {
                _logger.LogWarning($"{nameof(ProximityDevice)} is null");
                return NotFound();
            }

            return Partial("_DeleteProximityDevice", this);
        }

        public async Task<IActionResult> OnPostDeleteProximityDeviceAsync(WorkstationProximityVault proximityDevice)
        {
            if (proximityDevice == null)
            {
                _logger.LogWarning($"{nameof(proximityDevice)} is null");
                return NotFound();
            }

            try
            {
                await _workstationService.DeleteProximityVaultAsync(proximityDevice.Id);
                SuccessMessage = $"Proximity device removed.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            var id = proximityDevice.WorkstationId;
            return RedirectToPage("./Details", new { id });
        }
    }
}