using HES.Core.Entities;
using HES.Core.Models;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Utilities;

namespace HES.Web.Pages.Devices
{
    public class IndexModel : PageModel
    {
        private readonly IDeviceService _deviceService;
        private readonly IEmployeeService _employeeService;
        private readonly IOrgStructureService _orgStructureService;
        private readonly IWorkstationAuditService _workstationAuditService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly ILogger<IndexModel> _logger;

        public IList<DeviceAccessProfile> DeviceAccessProfiles { get; set; }
        public IList<Device> Devices { get; set; }
        public Device Device { get; set; }
        public DeviceFilter DeviceFilter { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(IDeviceService deviceService,
                          IEmployeeService employeeService,
                          IOrgStructureService orgStructureService,
                          IWorkstationAuditService workstationAuditService,
                          IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                          ILogger<IndexModel> logger)
        {
            _deviceService = deviceService;
            _employeeService = employeeService;
            _orgStructureService = orgStructureService;
            _workstationAuditService = workstationAuditService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            Devices = await _deviceService.GetDevicesAsync();

            ViewData["Firmware"] = new SelectList(Devices.Select(s => s.Firmware).Distinct().OrderBy(f => f).ToDictionary(t => t, t => t), "Key", "Value");
            ViewData["Employees"] = new SelectList(await _employeeService.Query().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.QueryOfCompany().ToListAsync(), "Id", "Name");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        #region From Dashboard

        public async Task OnGetLowBatteryAsync()
        {
            Devices = await _deviceService
                .QueryOfDevice()
                .Include(d => d.DeviceAccessProfile)
                .Include(d => d.Employee.Department.Company)
                .Where(d => d.Battery <= 30)
                .ToListAsync();

            ViewData["Firmware"] = new SelectList(Devices.Select(s => s.Firmware).Distinct().OrderBy(f => f).ToDictionary(t => t, t => t), "Key", "Value");
            ViewData["Employees"] = new SelectList(await _employeeService.Query().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.QueryOfCompany().ToListAsync(), "Id", "Name");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task OnGetDeviceLockedAsync()
        {
            Devices = await _deviceService
                .QueryOfDevice()
                .Include(d => d.DeviceAccessProfile)
                .Include(d => d.Employee.Department.Company)
                .Where(d => d.State == DeviceState.Locked)
                .ToListAsync();

            ViewData["Firmware"] = new SelectList(Devices.Select(s => s.Firmware).Distinct().OrderBy(f => f).ToDictionary(t => t, t => t), "Key", "Value");
            ViewData["Employees"] = new SelectList(await _employeeService.Query().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.QueryOfCompany().ToListAsync(), "Id", "Name");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task OnGetDeviceErrorAsync()
        {
            Devices = await _deviceService
                .QueryOfDevice()
                .Include(d => d.DeviceAccessProfile)
                .Include(d => d.Employee.Department.Company)
                .Where(d => d.State == DeviceState.Error)
                .ToListAsync();

            ViewData["Firmware"] = new SelectList(Devices.Select(s => s.Firmware).Distinct().OrderBy(f => f).ToDictionary(t => t, t => t), "Key", "Value");
            ViewData["Employees"] = new SelectList(await _employeeService.Query().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.QueryOfCompany().ToListAsync(), "Id", "Name");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task OnGetInReserveAsync()
        {
            Devices = await _deviceService
                .QueryOfDevice()
                .Include(d => d.DeviceAccessProfile)
                .Include(d => d.Employee.Department.Company)
                .Where(d => d.EmployeeId == null)
                .ToListAsync();

            ViewData["Firmware"] = new SelectList(Devices.Select(s => s.Firmware).Distinct().OrderBy(f => f).ToDictionary(t => t, t => t), "Key", "Value");
            ViewData["Employees"] = new SelectList(await _employeeService.Query().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.QueryOfCompany().ToListAsync(), "Id", "Name");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        #endregion

        public async Task<IActionResult> OnPostFilterDevicesAsync(DeviceFilter DeviceFilter)
        {
            Devices = await _deviceService.GetFilteredDevicesAsync(DeviceFilter);
            return Partial("_DevicesTable", this);
        }

        public async Task<IActionResult> OnGetEditDeviceRfidAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Device = await _deviceService.GetDeviceByIdAsync(id);

            if (Device == null)
            {
                _logger.LogWarning($"{nameof(Device)} is null");
                return NotFound();
            }

            return Partial("_EditDeviceRfid", this);
        }

        public async Task<IActionResult> OnPostEditDeviceRfidAsync(Device device)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = ValidationHepler.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Index");
            }

            try
            {
                await _deviceService.EditRfidAsync(device);
                SuccessMessage = $"RFID updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<JsonResult> OnGetJsonDepartmentAsync(string id)
        {
            return new JsonResult(await _orgStructureService.DepartmentQuery().Where(d => d.CompanyId == id).ToListAsync());
        }

        public async Task<IActionResult> OnGetSetProfileAsync()
        {
            DeviceAccessProfiles = await _deviceService.GetAccessProfilesAsync();
            return Partial("_SetProfile", this);
        }

        public async Task<IActionResult> OnPostSetProfileAsync(string[] devices, string profileId)
        {
            try
            {
                await _deviceService.SetProfileAsync(devices, profileId);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devices);
                SuccessMessage = $"New profile sent to server for processing.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetUnlockPinAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Device = await _deviceService.GetDeviceByIdAsync(id);

            if (Device == null)
            {
                _logger.LogWarning($"{nameof(Device)} is null");
                return NotFound();
            }

            return Partial("_UnlockPin", this);
        }

        public async Task<IActionResult> OnPostUnlockPinAsync(string deviceId)
        {
            if (deviceId == null)
            {
                _logger.LogWarning($"{nameof(deviceId)} is null");
                return NotFound();
            }

            try
            {
                await _deviceService.UnlockPinAsync(deviceId);
                await _workstationAuditService.AddPendingUnlockEventAsync(deviceId);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(deviceId);
                SuccessMessage = $"Pending unlock sent to server for processing.";
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