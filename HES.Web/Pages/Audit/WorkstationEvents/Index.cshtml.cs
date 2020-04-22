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
        private readonly IWorkstationAuditService _workstationAuditService;
        private readonly IWorkstationService _workstationService;
        private readonly IEmployeeService _employeeService;
        private readonly IDeviceService _deviceService;
        private readonly IAccountService _deviceAccountService;
        private readonly IOrgStructureService _orgStructureService;
        public IList<WorkstationEvent> WorkstationEvents { get; set; }
        public WorkstationEventFilter WorkstationEventFilter { get; set; }

        public IndexModel(IWorkstationAuditService workstationAuditService,
                          IWorkstationService workstationService,
                          IEmployeeService employeeService,
                          IDeviceService deviceService,
                          IAccountService deviceAccountService,
                          IOrgStructureService orgStructureService)
        {
            _workstationAuditService = workstationAuditService;
            _workstationService = workstationService;
            _employeeService = employeeService;
            _deviceService = deviceService;
            _deviceAccountService = deviceAccountService;
            _orgStructureService = orgStructureService;
        }

        public async Task OnGetAsync()
        {
            WorkstationEvents = await _workstationAuditService.GetWorkstationEventsAsync();

            ViewData["Events"] = new SelectList(Enum.GetValues(typeof(WorkstationEventType)).Cast<WorkstationEventType>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            ViewData["Severity"] = new SelectList(Enum.GetValues(typeof(WorkstationEventSeverity)).Cast<WorkstationEventSeverity>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            ViewData["Workstations"] = new SelectList(await _workstationService.WorkstationQuery().ToListAsync(), "Id", "Name");
            ViewData["Devices"] = new SelectList(await _deviceService.VaultQuery().ToListAsync(), "Id", "Id");
            ViewData["Employees"] = new SelectList(await _employeeService.EmployeeQuery().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["DeviceAccountTypes"] = new SelectList(Enum.GetValues(typeof(AccountType)).Cast<AccountType>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task<IActionResult> OnPostFilterWorkstationEventsAsync(WorkstationEventFilter workstationEventFilter)
        {
            WorkstationEvents = await _workstationAuditService.GetFilteredWorkstationEventsAsync(workstationEventFilter);
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

            currentAccounts.Insert(0, new Account() { Id = "active", Name = "Active" });

            var deletedAccounts = await _deviceAccountService
                          .Query()
                          .Where(d => d.EmployeeId == id && d.Deleted == true)
                          .OrderBy(d => d.Name)
                          .ToListAsync();

            deletedAccounts.Insert(0, new Account() { Id = "deleted", Name = "Deleted" });

            var accounts = currentAccounts.Concat(deletedAccounts);

            return new JsonResult(accounts);
        }
    }
}