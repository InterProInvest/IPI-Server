using HES.Core.Entities;
using HES.Core.Entities.Models;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSessions
{
    public class IndexModel : PageModel
    {
        private readonly IWorkstationSessionService _workstationSessionService;
        private readonly IWorkstationService _workstationService;
        private readonly IDeviceService _deviceService;
        private readonly IDeviceAccountService _deviceAccountService;
        private readonly IEmployeeService _employeeService;
        private readonly IOrgStructureService _orgStructureService;

        public IList<WorkstationSession> WorkstationSessions { get; set; }
        public WorkstationSessionFilter WorkstationSessionFilter { get; set; }

        public IndexModel(IWorkstationSessionService workstationSessionService,
                            IWorkstationService workstationService,
                            IDeviceService deviceService,
                            IDeviceAccountService deviceAccountService,
                            IEmployeeService employeeService,
                            IOrgStructureService orgStructureService)
        {
            _workstationSessionService = workstationSessionService;
            _workstationService = workstationService;
            _deviceService = deviceService;
            _deviceAccountService = deviceAccountService;
            _employeeService = employeeService;
            _orgStructureService = orgStructureService;

        }

        public async Task OnGetAsync()
        {
            WorkstationSessions = await _workstationSessionService
                .Query()
                .Include(w => w.Workstation)
                .Include(w => w.Device)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.DeviceAccount)
                .OrderByDescending(w => w.StartDate)
                .Take(500)
                .ToListAsync();

            ViewData["UnlockId"] = new SelectList(Enum.GetValues(typeof(SessionSwitchSubject)).Cast<SessionSwitchSubject>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            ViewData["Workstations"] = new SelectList(await _workstationService.Query().ToListAsync(), "Id", "Name");
            ViewData["Devices"] = new SelectList(await _deviceService.Query().ToListAsync(), "Id", "Id");
            ViewData["Employees"] = new SelectList(await _employeeService.Query().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["DeviceAccountTypes"] = new SelectList(Enum.GetValues(typeof(AccountType)).Cast<AccountType>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task OnGetNonHideezUnlockAsync()
        {
            WorkstationSessions = await _workstationSessionService
                .Query()
                .Include(w => w.Workstation)
                .Include(w => w.Device)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.DeviceAccount)
                .Where(w => w.StartDate >= DateTime.UtcNow.AddDays(-1) && w.UnlockedBy == Hideez.SDK.Communication.SessionSwitchSubject.NonHideez)
                .OrderByDescending(w => w.StartDate)
                .Take(500)
                .ToListAsync();

            ViewData["UnlockId"] = new SelectList(Enum.GetValues(typeof(SessionSwitchSubject)).Cast<SessionSwitchSubject>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            ViewData["Workstations"] = new SelectList(await _workstationService.Query().ToListAsync(), "Id", "Name");
            ViewData["Devices"] = new SelectList(await _deviceService.Query().ToListAsync(), "Id", "Id");
            ViewData["Employees"] = new SelectList(await _employeeService.Query().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["DeviceAccountTypes"] = new SelectList(Enum.GetValues(typeof(AccountType)).Cast<AccountType>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task OnGetLongOpenSessionAsync()
        {
            WorkstationSessions = await _workstationSessionService
                .Query()
                .Include(w => w.Workstation)
                .Include(w => w.Device)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.DeviceAccount)
                .Where(w => w.StartDate <= DateTime.UtcNow.AddHours(-12) && w.EndDate == null)
                .OrderByDescending(w => w.StartDate)
                .Take(500)
                .ToListAsync();

            ViewData["UnlockId"] = new SelectList(Enum.GetValues(typeof(SessionSwitchSubject)).Cast<SessionSwitchSubject>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            ViewData["Workstations"] = new SelectList(await _workstationService.Query().ToListAsync(), "Id", "Name");
            ViewData["Devices"] = new SelectList(await _deviceService.Query().ToListAsync(), "Id", "Id");
            ViewData["Employees"] = new SelectList(await _employeeService.Query().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["DeviceAccountTypes"] = new SelectList(Enum.GetValues(typeof(AccountType)).Cast<AccountType>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task OnGetOpenedSessionsAsync()
        {
            WorkstationSessions = await _workstationSessionService
                .Query()
                .Include(w => w.Workstation)
                .Include(w => w.Device)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.DeviceAccount)
                .Where(w => w.EndDate == null)
                .OrderByDescending(w => w.StartDate)
                .Take(500)
                .ToListAsync();

            ViewData["UnlockId"] = new SelectList(Enum.GetValues(typeof(SessionSwitchSubject)).Cast<SessionSwitchSubject>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            ViewData["Workstations"] = new SelectList(await _workstationService.Query().ToListAsync(), "Id", "Name");
            ViewData["Devices"] = new SelectList(await _deviceService.Query().ToListAsync(), "Id", "Id");
            ViewData["Employees"] = new SelectList(await _employeeService.Query().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["DeviceAccountTypes"] = new SelectList(Enum.GetValues(typeof(AccountType)).Cast<AccountType>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task<IActionResult> OnPostFilterWorkstationSessionsAsync(WorkstationSessionFilter WorkstationSessionFilter)
        {
            var filter = _workstationSessionService
                .Query()
                .Include(w => w.Workstation)
                .Include(w => w.Device)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.DeviceAccount)
                .AsQueryable();

            if (WorkstationSessionFilter.StartDate != null && WorkstationSessionFilter.EndDate != null)
            {
                filter = filter.Where(w => w.StartDate >= WorkstationSessionFilter.StartDate.Value.AddSeconds(0).AddMilliseconds(0).ToUniversalTime() &&
                                           w.EndDate <= WorkstationSessionFilter.EndDate.Value.AddSeconds(59).AddMilliseconds(999).ToUniversalTime());
            }
            if (WorkstationSessionFilter.UnlockId != null)
            {
                filter = filter.Where(w => w.UnlockedBy == (SessionSwitchSubject)WorkstationSessionFilter.UnlockId);
            }
            if (WorkstationSessionFilter.WorkstationId != null)
            {
                filter = filter.Where(w => w.WorkstationId == WorkstationSessionFilter.WorkstationId);
            }
            if (WorkstationSessionFilter.UserSession != null)
            {
                filter = filter.Where(w => w.UserSession.Contains(WorkstationSessionFilter.UserSession));
            }
            if (WorkstationSessionFilter.DeviceId != null)
            {
                filter = filter.Where(w => w.Device.Id == WorkstationSessionFilter.DeviceId);
            }
            if (WorkstationSessionFilter.EmployeeId != null)
            {
                filter = filter.Where(w => w.EmployeeId == WorkstationSessionFilter.EmployeeId);
            }
            if (WorkstationSessionFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Department.Company.Id == WorkstationSessionFilter.CompanyId);
            }
            if (WorkstationSessionFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.DepartmentId == WorkstationSessionFilter.DepartmentId);
            }
            if (WorkstationSessionFilter.DeviceAccountId != null)
            {
                filter = filter.Where(w => w.DeviceAccountId == WorkstationSessionFilter.DeviceAccountId);
            }
            if (WorkstationSessionFilter.DeviceAccountTypeId != null)
            {
                filter = filter.Where(w => w.DeviceAccount.Type == (AccountType)WorkstationSessionFilter.DeviceAccountTypeId);
            }

            WorkstationSessions = await filter
                .OrderByDescending(w => w.StartDate)
                .Take(WorkstationSessionFilter.Records)
                .ToListAsync();

            return Partial("_WorkstationSessionsTable", this);
        }

        public async Task<JsonResult> OnGetJsonDepartmentAsync(string id)
        {
            return new JsonResult(await _orgStructureService.DepartmentQuery().Where(d => d.CompanyId == id).ToListAsync());
        }

        public async Task<JsonResult> OnGetJsonDeviceAccountsAsync(string id)
        {
            var currentAccounts = await _deviceAccountService
                .Query()
                .Where(d => d.EmployeeId == id && d.Deleted == false)
                .OrderBy(d => d.Name)
                .ToListAsync();

            currentAccounts.Insert(0, new DeviceAccount() { Id = "active", Name = "Active" });

            var deletedAccounts = await _deviceAccountService
                          .Query()
                          .Where(d => d.EmployeeId == id && d.Deleted == true)
                          .OrderBy(d => d.Name)
                          .ToListAsync();

            deletedAccounts.Insert(0, new DeviceAccount() { Id = "deleted", Name = "Deleted" });

            var accounts = currentAccounts.Concat(deletedAccounts);

            return new JsonResult(accounts);
        }
    }
}