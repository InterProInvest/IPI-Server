using HES.Core.Interfaces;
using HES.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSummaries
{
    public class IndexModel : PageModel
    {
        private readonly IWorkstationAuditService _workstationAuditService;
        private readonly IEmployeeService _employeeService;
        private readonly IOrgStructureService _orgStructureService;
        public IList<SummaryByDayAndEmployee> SummaryByDayAndEmployee { get; set; }
        public IList<SummaryByEmployees> SummaryByEmployees { get; set; }
        public IList<SummaryByDepartments> SummaryByDepartments { get; set; }
        public IList<SummaryByWorkstations> SummaryByWorkstations { get; set; }
        public SummaryFilter SummaryFilter { get; set; }

        public IndexModel(IWorkstationAuditService workstationAuditService,
                            IEmployeeService employeeService,
                            IOrgStructureService orgStructureService)
        {
            _workstationAuditService = workstationAuditService;
            _employeeService = employeeService;
            _orgStructureService = orgStructureService;
        }

        public async Task OnGet()
        {
            SummaryByDayAndEmployee = await _workstationAuditService.GetSummaryByDayAndEmployeesAsync();

            ViewData["Employees"] = new SelectList(await _employeeService.Query().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.QueryOfCompany().ToListAsync(), "Id", "Name");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task<IActionResult> OnPostFilterSummaryByDaysAndEmployeesAsync(SummaryFilter summaryFilter)
        {
            SummaryByDayAndEmployee = await _workstationAuditService.GetFilteredSummaryByDaysAndEmployeesAsync(summaryFilter);
            return Partial("_ByDaysAndEmployees", this);
        }

        public async Task<IActionResult> OnPostFilterSummaryByEmployeesAsync(SummaryFilter summaryFilter)
        {
            SummaryByEmployees = await _workstationAuditService.GetFilteredSummaryByEmployeesAsync(summaryFilter);
            return Partial("_ByEmployees", this);
        }

        public async Task<IActionResult> OnPostFilterSummaryByDepartmentsAsync(SummaryFilter summaryFilter)
        {
            SummaryByDepartments = await _workstationAuditService.GetFilteredSummaryByDepartmentsAsync(summaryFilter);
            return Partial("_ByDepartments", this);
        }

        public async Task<IActionResult> OnPostFilterSummaryByWorkstationsAsync(SummaryFilter summaryFilter)
        {
            SummaryByWorkstations = await _workstationAuditService.GetFilteredSummaryByWorkstationsAsync(summaryFilter);
            return Partial("_ByWorkstations", this);
        }

        public async Task<JsonResult> OnGetJsonDepartmentAsync(string id)
        {
            return new JsonResult(await _orgStructureService.DepartmentQuery().Where(d => d.CompanyId == id).ToListAsync());
        }
    }
}