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

namespace HES.Web.Pages.Devices
{
    public class IndexModel : PageModel
    {
        private readonly IDeviceService _deviceService;
        private readonly IEmployeeService _employeeService;
        private readonly IDeviceAccessProfilesService _deviceAccessProfilesService;
        private readonly IRemoteTaskService _remoteTaskService;
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
                          IDeviceAccessProfilesService deviceAccessProfilesService,
                          IRemoteTaskService remoteTaskService,
                          IOrgStructureService orgStructureService,
                          IWorkstationAuditService workstationAuditService,
                          IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                          ILogger<IndexModel> logger)
        {
            _deviceService = deviceService;
            _employeeService = employeeService;
            _deviceAccessProfilesService = deviceAccessProfilesService;
            _remoteTaskService = remoteTaskService;
            _orgStructureService = orgStructureService;
            _workstationAuditService = workstationAuditService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            Devices = await _deviceService
                .Query()
                .Include(d => d.DeviceAccessProfile)
                .Include(d => d.Employee.Department.Company)
                .ToListAsync();

            ViewData["Firmware"] = new SelectList(Devices.Select(s => s.Firmware).Distinct().OrderBy(f => f).ToDictionary(t => t, t => t), "Key", "Value");
            ViewData["Employees"] = new SelectList(await _employeeService.Query().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task OnGetLowBatteryAsync()
        {
            Devices = await _deviceService
                .Query()
                .Include(d => d.DeviceAccessProfile)
                .Include(d => d.Employee.Department.Company)
                .Where(d => d.Battery <= 30)
                .ToListAsync();

            ViewData["Firmware"] = new SelectList(Devices.Select(s => s.Firmware).Distinct().OrderBy(f => f).ToDictionary(t => t, t => t), "Key", "Value");
            ViewData["Employees"] = new SelectList(await _employeeService.Query().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task OnGetDeviceLockedAsync()
        {
            Devices = await _deviceService
                .Query()
                .Include(d => d.DeviceAccessProfile)
                .Include(d => d.Employee.Department.Company)
                .Where(d => d.State == DeviceState.Locked)
                .ToListAsync();

            ViewData["Firmware"] = new SelectList(Devices.Select(s => s.Firmware).Distinct().OrderBy(f => f).ToDictionary(t => t, t => t), "Key", "Value");
            ViewData["Employees"] = new SelectList(await _employeeService.Query().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task OnGetDeviceErrorAsync()
        {
            Devices = await _deviceService
                .Query()
                .Include(d => d.DeviceAccessProfile)
                .Include(d => d.Employee.Department.Company)
                .Where(d => d.State == DeviceState.Error)
                .ToListAsync();

            ViewData["Firmware"] = new SelectList(Devices.Select(s => s.Firmware).Distinct().OrderBy(f => f).ToDictionary(t => t, t => t), "Key", "Value");
            ViewData["Employees"] = new SelectList(await _employeeService.Query().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task OnGetInReserveAsync()
        {
            Devices = await _deviceService
                .Query()
                .Include(d => d.DeviceAccessProfile)
                .Include(d => d.Employee.Department.Company)
                .Where(d => d.EmployeeId == null)
                .ToListAsync();

            ViewData["Firmware"] = new SelectList(Devices.Select(s => s.Firmware).Distinct().OrderBy(f => f).ToDictionary(t => t, t => t), "Key", "Value");
            ViewData["Employees"] = new SelectList(await _employeeService.Query().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task<IActionResult> OnPostFilterDevicesAsync(DeviceFilter DeviceFilter)
        {
            var filter = _deviceService
                .Query()
                .Include(d => d.DeviceAccessProfile)
                .Include(c => c.Employee.Department.Company)
                .AsQueryable();

            if (DeviceFilter.Battery != null)
            {
                filter = filter.Where(w => w.Battery == DeviceFilter.Battery);
            }
            if (DeviceFilter.Firmware != null)
            {
                filter = filter.Where(w => w.Firmware.Contains(DeviceFilter.Firmware));
            }
            if (DeviceFilter.EmployeeId != null)
            {
                if (DeviceFilter.EmployeeId == "N/A")
                {
                    filter = filter.Where(w => w.EmployeeId == null);
                }
                else
                {
                    filter = filter.Where(w => w.EmployeeId == DeviceFilter.EmployeeId);
                }
            }
            if (DeviceFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Employee.Department.Company.Id == DeviceFilter.CompanyId);
            }
            if (DeviceFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.Employee.DepartmentId == DeviceFilter.DepartmentId);
            }
            if (DeviceFilter.StartDate != null && DeviceFilter.EndDate != null)
            {
                filter = filter.Where(w => w.LastSynced.HasValue
                                        && w.LastSynced.Value >= DeviceFilter.StartDate.Value.AddSeconds(0).AddMilliseconds(0).ToUniversalTime()
                                        && w.LastSynced.Value <= DeviceFilter.EndDate.Value.AddSeconds(59).AddMilliseconds(999).ToUniversalTime());
            }

            Devices = await filter
                .OrderBy(w => w.Id)
                .Take(DeviceFilter.Records)
                .ToListAsync();

            return Partial("_DevicesTable", this);
        }

        public async Task<IActionResult> OnGetEditDeviceRfidAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Device = await _deviceService.Query()
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Device == null)
            {
                _logger.LogWarning("Device == null");
                return NotFound();
            }

            return Partial("_EditDeviceRfid", this);
        }

        public async Task<IActionResult> OnPostEditDeviceRfidAsync(Device device)
        {
            if (device == null)
            {
                _logger.LogWarning("device == null");
                return NotFound();
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
            DeviceAccessProfiles = await _deviceAccessProfilesService.Query().ToListAsync();
            return Partial("_SetProfile", this);
        }

        public async Task<IActionResult> OnPostSetProfileAsync(string[] devices, string profileId)
        {
            if (devices == null && profileId == null)
            {
                _logger.LogWarning("devices == null && profileId == null");
                return RedirectToPage("./Index");
            }

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
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Device = await _deviceService.Query()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Device == null)
            {
                _logger.LogWarning("Device == null");
                return NotFound();
            }

            return Partial("_UnlockPin", this);
        }

        public async Task<IActionResult> OnPostUnlockPinAsync(string deviceId)
        {
            if (deviceId == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            try
            {
                await _deviceService.UnlockPinAsync(deviceId);
                await _workstationAuditService.AddPendingUnlockEventAsync(deviceId);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(deviceId);
                SuccessMessage = $"Pending unlock.";
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