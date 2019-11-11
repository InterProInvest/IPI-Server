using HES.Core.Entities;
using HES.Core.Models;
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


namespace HES.Web.Pages.Audit.WorkstationEvents
{
    public class IndexModel : PageModel
    {
        private readonly IWorkstationEventService _workstationEventService;
        private readonly IWorkstationService _workstationService;
        private readonly IEmployeeService _employeeService;
        private readonly IDeviceService _deviceService;
        private readonly IDeviceAccountService _deviceAccountService;
        private readonly IOrgStructureService _orgStructureService;
        public IList<WorkstationEvent> WorkstationEvents { get; set; }
        public WorkstationEventFilter WorkstationEventFilter { get; set; }

        public IndexModel(IWorkstationEventService workstationEventService,
                          IWorkstationService workstationService,
                          IEmployeeService employeeService,
                          IDeviceService deviceService,
                          IDeviceAccountService deviceAccountService,
                          IOrgStructureService orgStructureService)
        {
            _workstationEventService = workstationEventService;
            _workstationService = workstationService;
            _employeeService = employeeService;
            _deviceService = deviceService;
            _deviceAccountService = deviceAccountService;
            _orgStructureService = orgStructureService;
        }

        public async Task OnGetAsync()
        {
            WorkstationEvents = await _workstationEventService
                .Query()
                .Include(w => w.Workstation)
                .Include(w => w.Device)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.DeviceAccount)
                .OrderByDescending(w => w.Date)
                .Take(500)
                .ToListAsync();

            ViewData["Events"] = new SelectList(Enum.GetValues(typeof(WorkstationEventType)).Cast<WorkstationEventType>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            ViewData["Severity"] = new SelectList(Enum.GetValues(typeof(WorkstationEventSeverity)).Cast<WorkstationEventSeverity>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            ViewData["Workstations"] = new SelectList(await _workstationService.QueryOfWorkstation().ToListAsync(), "Id", "Name");
            ViewData["Devices"] = new SelectList(await _deviceService.Query().ToListAsync(), "Id", "Id");
            ViewData["Employees"] = new SelectList(await _employeeService.Query().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["DeviceAccountTypes"] = new SelectList(Enum.GetValues(typeof(AccountType)).Cast<AccountType>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task<IActionResult> OnPostFilterWorkstationEventsAsync(WorkstationEventFilter WorkstationEventFilter)
        {
            var filter = _workstationEventService
                 .Query()
                 .Include(w => w.Workstation)
                 .Include(w => w.Device)
                 .Include(w => w.Employee)
                 .Include(w => w.Department.Company)
                 .Include(w => w.DeviceAccount)
                 .AsQueryable();

            if (WorkstationEventFilter.StartDate != null && WorkstationEventFilter.EndDate != null)
            {
                filter = filter.Where(w => w.Date >= WorkstationEventFilter.StartDate.Value.Date.AddSeconds(0).AddMilliseconds(0).ToUniversalTime() &&
                                           w.Date <= WorkstationEventFilter.EndDate.Value.Date.AddSeconds(59).AddMilliseconds(999).ToUniversalTime());
            }
            if (WorkstationEventFilter.EventId != null)
            {
                filter = filter.Where(w => w.EventId == (WorkstationEventType)WorkstationEventFilter.EventId);
            }
            if (WorkstationEventFilter.SeverityId != null)
            {
                filter = filter.Where(w => w.SeverityId == (WorkstationEventSeverity)WorkstationEventFilter.SeverityId);
            }
            if (WorkstationEventFilter.Note != null)
            {
                filter = filter.Where(w => w.Note.Contains(WorkstationEventFilter.Note));
            }
            if (WorkstationEventFilter.WorkstationId != null)
            {
                filter = filter.Where(w => w.WorkstationId == WorkstationEventFilter.WorkstationId);
            }
            if (WorkstationEventFilter.UserSession != null)
            {
                filter = filter.Where(w => w.UserSession.Contains(WorkstationEventFilter.UserSession));
            }
            if (WorkstationEventFilter.DeviceId != null)
            {
                filter = filter.Where(w => w.Device.Id == WorkstationEventFilter.DeviceId);
            }
            if (WorkstationEventFilter.EmployeeId != null)
            {
                filter = filter.Where(w => w.EmployeeId == WorkstationEventFilter.EmployeeId);
            }
            if (WorkstationEventFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Department.Company.Id == WorkstationEventFilter.CompanyId);
            }
            if (WorkstationEventFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.DepartmentId == WorkstationEventFilter.DepartmentId);
            }
            if (WorkstationEventFilter.DeviceAccountId != null)
            {
                filter = filter.Where(w => w.DeviceAccountId == WorkstationEventFilter.DeviceAccountId);
            }
            if (WorkstationEventFilter.DeviceAccountTypeId != null)
            {
                filter = filter.Where(w => w.DeviceAccount.Type == (AccountType)WorkstationEventFilter.DeviceAccountTypeId);
            }

            WorkstationEvents = await filter
                .OrderByDescending(w => w.Date)
                .Take(WorkstationEventFilter.Records)
                .ToListAsync();

            return Partial("_WorkstationEventsTable", this);
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