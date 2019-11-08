using HES.Core.Entities;
using HES.Core.Interfaces;
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
        private readonly IProximityDeviceService _workstationProximityDeviceService;
        private readonly IEmployeeService _employeeService;
        private readonly IDeviceService _deviceService;
        private readonly ILogger<DetailsModel> _logger;

        public IList<ProximityDevice> WorkstationProximityDevices { get; set; }
        public IList<Device> Devices { get; set; }
        public Workstation Workstation { get; set; }
        public ProximityDevice WorkstationProximityDevice { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public DetailsModel(IWorkstationService workstationService,
                            IProximityDeviceService workstationProximityDeviceService,
                            IEmployeeService employeeService,
                            IDeviceService deviceService,
                            ILogger<DetailsModel> logger)
        {
            _workstationService = workstationService;
            _workstationProximityDeviceService = workstationProximityDeviceService;
            _employeeService = employeeService;
            _deviceService = deviceService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Workstation = await _workstationService
                .Query()
                .Include(c => c.Department.Company)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (Workstation == null)
            {
                _logger.LogWarning("Workstation == null");
                return NotFound();
            }

            WorkstationProximityDevices = await _workstationProximityDeviceService
                .Query()
                .Include(d => d.Device.Employee.Department.Company)
                .Where(d => d.WorkstationId == id)
                .ToListAsync();

            if (WorkstationProximityDevices == null)
            {
                _logger.LogWarning("WorkstationProximityDevices == null");
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnGetAddProximityDeviceAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Workstation = await _workstationService
            .Query()
            .FirstOrDefaultAsync(m => m.Id == id);

            if (Workstation == null)
            {
                _logger.LogWarning("Workstation == null");
                return NotFound();
            }

            WorkstationProximityDevices = await _workstationProximityDeviceService
                .Query()
                .Where(d => d.WorkstationId == id)
                .ToListAsync();

            var deviceQuery = _deviceService.Query().AsQueryable();

            foreach (var proximityDevice in WorkstationProximityDevices)
            {
                deviceQuery = deviceQuery.Where(d => d.Id != proximityDevice.DeviceId);
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
                _logger.LogWarning("workstationId == null");
                return NotFound();
            }

            try
            {
                await _workstationProximityDeviceService.AddProximityDeviceAsync(workstationId, devicesId);
                await _workstationProximityDeviceService.UpdateProximitySettingsAsync(workstationId);

                if (devicesId.Length > 1)
                {
                    var devices = string.Empty;
                    foreach (var item in devicesId)
                    {
                        devices += item + Environment.NewLine;
                    }
                    SuccessMessage = $"Devices: {devices} added.";
                }
                else
                {
                    SuccessMessage = $"Device {devicesId[0]} added.";
                }
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
                _logger.LogWarning("id == null");
                return NotFound();
            }

            WorkstationProximityDevice = await _workstationProximityDeviceService
                .Query()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (WorkstationProximityDevice == null)
            {
                _logger.LogWarning("ProximityDevice == null");
                return NotFound();
            }

            return Partial("_EditProximitySettings", this);
        }

        public async Task<IActionResult> OnPostEditProximitySettingsAsync(ProximityDevice WorkstationProximityDevice)
        {
            var id = WorkstationProximityDevice.WorkstationId;
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model is not valid");
                return RedirectToPage("./Details", new { id });
            }

            try
            {
                await _workstationProximityDeviceService.EditProximityDeviceAsync(WorkstationProximityDevice);
                await _workstationProximityDeviceService.UpdateProximitySettingsAsync(WorkstationProximityDevice.WorkstationId);

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
                _logger.LogWarning("id == null");
                return NotFound();
            }

            WorkstationProximityDevice = await _workstationProximityDeviceService
                .Query()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (WorkstationProximityDevice == null)
            {
                _logger.LogWarning("ProximityDevice == null");
                return NotFound();
            }

            return Partial("_DeleteProximityDevice", this);
        }

        public async Task<IActionResult> OnPostDeleteProximityDeviceAsync(ProximityDevice WorkstationProximityDevice)
        {
            if (WorkstationProximityDevice == null)
            {
                _logger.LogWarning("PoximityDevice == null");
                return NotFound();
            }

            try
            {
                await _workstationProximityDeviceService.DeleteProximityDeviceAsync(WorkstationProximityDevice.Id);
                SuccessMessage = $"Device removed.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            var id = WorkstationProximityDevice.WorkstationId;
            return RedirectToPage("./Details", new { id });
        }
    }
}