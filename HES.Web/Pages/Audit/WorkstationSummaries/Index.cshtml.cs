using HES.Core.Entities.Models;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSummaries
{
    public class IndexModel : PageModel
    {
        private readonly IWorkstationSessionService _workstationSessionService;
        private readonly IEmployeeService _employeeService;
        private readonly IOrgStructureService _orgStructureService;
        public IList<SummaryByDayAndEmployee> SummaryByDayAndEmployee { get; set; }
        public IList<SummaryByEmployees> SummaryByEmployees { get; set; }
        public IList<SummaryByDepartments> SummaryByDepartments { get; set; }
        public IList<SummaryByWorkstations> SummaryByWorkstations { get; set; }
        public SummaryFilter SummaryFilter { get; set; }

        public IndexModel(IWorkstationSessionService workstationSessionService,
                            IEmployeeService employeeService,
                            IOrgStructureService orgStructureService)
        {
            _workstationSessionService = workstationSessionService;
            _employeeService = employeeService;
            _orgStructureService = orgStructureService;
        }

        public async Task OnGet()
        {
            SummaryByDayAndEmployee = await _workstationSessionService
                .SummaryByDayAndEmployeeSqlQuery
                ($@"SELECT
	                    DATE(workstationsessions.StartDate) AS Date,
	                    employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(employees.FirstName,' ',employees.LastName), 'N/A') AS Employee,
	                    companies.Id AS CompanyId,
	                    IFNULL(companies.Name, 'N/A') AS Company,
	                    departments.Id AS DepartmentId,
	                    IFNULL(departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS AvgSessionsDuration,
	                    COUNT(*) AS SessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS TotalSessionsDuration
                    FROM workstationsessions
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id
                    GROUP BY
	                    DATE(workstationsessions.StartDate),
	                    workstationsessions.EmployeeId
                    ORDER BY
	                    DATE(workstationsessions.StartDate) DESC, Employee ASC
                    LIMIT 500")
                .AsNoTracking()
                .ToListAsync();

            ViewData["Employees"] = new SelectList(await _employeeService.Query().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task<IActionResult> OnPostFilterSummaryByDaysAndEmployeesAsync(SummaryFilter SummaryFilter)
        {
            var where = string.Empty;
            List<string> parameters = new List<string>();

            if (SummaryFilter.StartDate != null && SummaryFilter.EndDate != null)
            {
                parameters.Add($"workstationsessions.StartDate BETWEEN '{SummaryFilter.StartDate.Value.AddSeconds(0).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}' AND '{SummaryFilter.EndDate.Value.AddSeconds(59).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            if (SummaryFilter.EmployeeId != null)
            {
                if (SummaryFilter.EmployeeId == "N/A")
                {
                    parameters.Add($"employees.Id IS NULL");
                }
                else
                {
                    parameters.Add($"employees.Id = '{SummaryFilter.EmployeeId}'");
                }
            }
            if (SummaryFilter.CompanyId != null)
            {
                if (SummaryFilter.CompanyId == "N/A")
                {
                    parameters.Add($"companies.Id IS NULL");
                }
                else
                {
                    parameters.Add($"companies.Id = '{SummaryFilter.CompanyId}'");
                }
            }
            if (SummaryFilter.DepartmentId != null)
            {
                if (SummaryFilter.DepartmentId == "N/A")
                {
                    parameters.Add($"departments.Id IS NULL");
                }
                else
                {
                    parameters.Add($"departments.Id = '{SummaryFilter.DepartmentId}'");
                }
            }

            if (parameters.Count > 0)
            {
                where = string.Join(" AND ", parameters).Insert(0, "WHERE ");
            }

            if (SummaryFilter.Records == 0)
            {
                SummaryFilter.Records = 500;
            }

            SummaryByDayAndEmployee = await _workstationSessionService
                .SummaryByDayAndEmployeeSqlQuery
                ($@"SELECT
	                    DATE(workstationsessions.StartDate) AS Date,
	                    employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(employees.FirstName,' ',employees.LastName), 'N/A') AS Employee,
	                    companies.Id AS CompanyId,
	                    IFNULL(companies.Name, 'N/A') AS Company,
	                    departments.Id AS DepartmentId,
	                    IFNULL(departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS AvgSessionsDuration,
	                    COUNT(*) AS SessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS TotalSessionsDuration
                    FROM workstationsessions
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id
                {where}
                    GROUP BY
	                    DATE(workstationsessions.StartDate),
	                    workstationsessions.EmployeeId
                    ORDER BY
	                    DATE(workstationsessions.StartDate) DESC, Employee ASC
                    LIMIT {SummaryFilter.Records}")
                .AsNoTracking()
                .ToListAsync();

            return Partial("_ByDaysAndEmployees", this);
        }

        public async Task<IActionResult> OnPostFilterSummaryByEmployeesAsync(SummaryFilter SummaryFilter)
        {
            var where = string.Empty;
            List<string> parameters = new List<string>();

            if (SummaryFilter.StartDate != null && SummaryFilter.EndDate != null)
            {
                parameters.Add($"workstationsessions.StartDate BETWEEN '{SummaryFilter.StartDate.Value.AddSeconds(0).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}' AND '{SummaryFilter.EndDate.Value.AddSeconds(59).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}'");
            }           
            if (SummaryFilter.CompanyId != null)
            {
                if (SummaryFilter.CompanyId == "N/A")
                {
                    parameters.Add($"companies.Id IS NULL");
                }
                else
                {
                    parameters.Add($"companies.Id = '{SummaryFilter.CompanyId}'");
                }
            }
            if (SummaryFilter.DepartmentId != null)
            {
                if (SummaryFilter.DepartmentId == "N/A")
                {
                    parameters.Add($"departments.Id IS NULL");
                }
                else
                {
                    parameters.Add($"departments.Id = '{SummaryFilter.DepartmentId}'");
                }
            }

            if (parameters.Count > 0)
            {
                where = string.Join(" AND ", parameters).Insert(0, "WHERE ");
            }

            if (SummaryFilter.Records == 0)
            {
                SummaryFilter.Records = 500;
            }

            SummaryByEmployees = await _workstationSessionService
                .SummaryByEmployeesSqlQuery
                ($@"SELECT
	                    employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(employees.FirstName,' ',employees.LastName), 'N/A') AS Employee,
	                    companies.Id AS CompanyId,
	                    IFNULL(companies.Name, 'N/A') AS Company,
	                    departments.Id AS DepartmentId,
	                    IFNULL(departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    COUNT(DISTINCT DATE(workstationsessions.StartDate)) AS WorkingDaysCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS AvgSessionsDuration,	
	                    COUNT(*) / COUNT(DISTINCT DATE(workstationsessions.StartDate)) AS AvgSessionsCountPerDay,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate))) / COUNT(DISTINCT DATE(workstationsessions.StartDate))) AS AvgWorkingHoursPerDay
                    FROM workstationsessions
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id
                {where}
                    GROUP BY
	                    workstationsessions.EmployeeId
                    ORDER BY
	                    Employee ASC
                    LIMIT {SummaryFilter.Records}")
                .AsNoTracking()
                .ToListAsync();

            return Partial("_ByEmployees", this);
        }

        public async Task<IActionResult> OnPostFilterSummaryByDepartmentsAsync(SummaryFilter SummaryFilter)
        {
            var where = string.Empty;
            List<string> parameters = new List<string>();

            if (SummaryFilter.StartDate != null && SummaryFilter.EndDate != null)
            {
                parameters.Add($"workstationsessions.StartDate BETWEEN '{SummaryFilter.StartDate.Value.AddSeconds(0).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}' AND '{SummaryFilter.EndDate.Value.AddSeconds(59).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}'");
            }

            if (SummaryFilter.CompanyId != null)
            {
                if (SummaryFilter.CompanyId == "N/A")
                {
                    parameters.Add($"companies.Id IS NULL");
                }
                else
                {
                    parameters.Add($"companies.Id = '{SummaryFilter.CompanyId}'");
                }
            }

            if (parameters.Count > 0)
            {
                where = string.Join(" AND ", parameters).Insert(0, "WHERE ");
            }

            if (SummaryFilter.Records == 0)
            {
                SummaryFilter.Records = 500;
            }

            SummaryByDepartments = await _workstationSessionService
                .SummaryByDepartmentsSqlQuery
                ($@"SELECT
	                    companies.Id AS CompanyId,
	                    IFNULL(companies.Name, 'N/A') AS Company,
	                    departments.Id AS DepartmentId,
	                    IFNULL(departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT IFNULL(employees.Id, 'N/A')) AS EmployeesCount,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS AvgSessionsDuration,	
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate))) / COUNT(DISTINCT IFNULL(employees.Id, 'N/A'))) AS AvgTotalDuartionByEmployee,
	                    COUNT(*) / COUNT(DISTINCT IFNULL(employees.Id, 'N/A')) AS AvgTotalSessionsCountByEmployee
                    FROM workstationsessions
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id
                {where}
                    GROUP BY
	                    departments.Id
                    ORDER BY
	                    Company ASC, Department ASC
                    LIMIT {SummaryFilter.Records}")
                .AsNoTracking()
                .ToListAsync();

            return Partial("_ByDepartments", this);
        }

        public async Task<IActionResult> OnPostFilterSummaryByWorkstationsAsync(SummaryFilter SummaryFilter)
        {
            var where = string.Empty;
            List<string> parameters = new List<string>();

            if (SummaryFilter.StartDate != null && SummaryFilter.EndDate != null)
            {
                parameters.Add($"workstationsessions.StartDate BETWEEN '{SummaryFilter.StartDate.Value.AddSeconds(0).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}' AND '{SummaryFilter.EndDate.Value.AddSeconds(59).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            if (SummaryFilter.EmployeeId != null)
            {
                if (SummaryFilter.EmployeeId == "N/A")
                {
                    parameters.Add($"employees.Id IS NULL");
                }
                else
                {
                    parameters.Add($"employees.Id = '{SummaryFilter.EmployeeId}'");
                }
            }
            if (SummaryFilter.CompanyId != null)
            {
                if (SummaryFilter.CompanyId == "N/A")
                {
                    parameters.Add($"companies.Id IS NULL");
                }
                else
                {
                    parameters.Add($"companies.Id = '{SummaryFilter.CompanyId}'");
                }
            }
            if (SummaryFilter.DepartmentId != null)
            {
                if (SummaryFilter.DepartmentId == "N/A")
                {
                    parameters.Add($"departments.Id IS NULL");
                }
                else
                {
                    parameters.Add($"departments.Id = '{SummaryFilter.DepartmentId}'");
                }
            }

            if (parameters.Count > 0)
            {
                where = string.Join(" AND ", parameters).Insert(0, "WHERE ");
            }

            if (SummaryFilter.Records == 0)
            {
                SummaryFilter.Records = 500;
            }

            SummaryByWorkstations = await _workstationSessionService
                .SummaryByWorkstationsSqlQuery
                ($@"SELECT
	                    workstations.Name AS Workstation,
	                    COUNT(DISTINCT IFNULL(companies.Id, 'N/A')) AS CompaniesCount,
	                    COUNT(DISTINCT IFNULL(departments.Id, 'N/A')) AS DepartmentsCount,
	                    COUNT(DISTINCT IFNULL(employees.Id, 'N/A')) AS EmployeesCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS AvgSessionsDuration,	
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate))) / COUNT(DISTINCT IFNULL(employees.Id, 'N/A'))) AS AvgTotalDuartionByEmployee,
	                    COUNT(*) / COUNT(DISTINCT IFNULL(employees.Id, 'N/A')) AS AvgTotalSessionsCountByEmployee
                    FROM workstationsessions
	                    LEFT JOIN workstations ON workstationsessions.WorkstationId = workstations.Id
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id
                {where}
                    GROUP BY
	                    WorkstationId
                    LIMIT {SummaryFilter.Records}")
                .AsNoTracking()
                .ToListAsync();

            return Partial("_ByWorkstations", this);
        }

        public async Task<JsonResult> OnGetJsonDepartmentAsync(string id)
        {
            return new JsonResult(await _orgStructureService.DepartmentQuery().Where(d => d.CompanyId == id).ToListAsync());
        }
    }
}