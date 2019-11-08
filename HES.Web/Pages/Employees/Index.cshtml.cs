﻿using HES.Core.Entities;
using HES.Core.Entities.Models;
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

namespace HES.Web.Pages.Employees
{
    public class IndexModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDeviceService _deviceService;
        private readonly IWorkstationService _workstationService;
        private readonly IProximityDeviceService _proximityDeviceService;
        private readonly IOrgStructureService _orgStructureService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly ISharedAccountService _sharedAccountService;
        private readonly ILogger<IndexModel> _logger;

        public IList<Employee> Employees { get; set; }
        public IList<Device> Devices { get; set; }
        public IList<Workstation> Workstations { get; set; }
        public Employee Employee { get; set; }
        public EmployeeFilter EmployeeFilter { get; set; }
        public Wizard Wizard { get; set; }
        public Company Company { get; set; }
        public Department Department { get; set; }
        public Position Position { get; set; }

        public bool HasForeignKey { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(IEmployeeService employeeService,
                          IDeviceService deviceService,
                          IWorkstationService workstationService,
                          IProximityDeviceService workstationProximityDeviceService,
                          IOrgStructureService orgStructureService,
                          IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                          ISharedAccountService sharedAccountService,
                          ILogger<IndexModel> logger)
        {
            _employeeService = employeeService;
            _deviceService = deviceService;
            _workstationService = workstationService;
            _proximityDeviceService = workstationProximityDeviceService;
            _orgStructureService = orgStructureService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _sharedAccountService = sharedAccountService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            Employees = await _employeeService.GetAllEmployees();

            ViewData["Companies"] = new SelectList(await _orgStructureService.CompanyQuery().OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["Positions"] = new SelectList(await _orgStructureService.PositionQuery().OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["DevicesCount"] = new SelectList(Employees.Select(s => s.Devices.Count()).Distinct().OrderBy(f => f).ToDictionary(t => t, t => t), "Key", "Value");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        #region Employee

        public async Task<IActionResult> OnPostFilterEmployeesAsync(EmployeeFilter employeeFilter)
        {
            Employees = await _employeeService.GetFilteredEmployees(employeeFilter);

            return Partial("_EmployeesTable", this);
        }

        public async Task<IActionResult> OnGetCreateEmployee()
        {
            ViewData["CompanyId"] = new SelectList(await _orgStructureService.CompanyQuery().OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["PositionId"] = new SelectList(await _orgStructureService.PositionQuery().OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["DeviceId"] = new SelectList(await _deviceService.Query().Where(d => d.EmployeeId == null).ToListAsync(), "Id", "Id");
            ViewData["WorkstationId"] = new SelectList(await _workstationService.Query().ToListAsync(), "Id", "Name");
            ViewData["WorkstationAccountType"] = new SelectList(Enum.GetValues(typeof(WorkstationAccountType)).Cast<WorkstationAccountType>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            ViewData["WorkstationAccounts"] = new SelectList(await _sharedAccountService.Query().Where(s => s.Kind == AccountKind.Workstation && s.Deleted == false).OrderBy(c => c.Name).ToListAsync(), "Id", "Name");

            Devices = await _deviceService
               .Query()
               .Where(d => d.EmployeeId == null)
               .ToListAsync();

            Workstations = await _workstationService
                .Query()
                .ToListAsync();

            return Partial("_CreateEmployee", this);
        }

        public async Task<IActionResult> OnPostCreateEmployeeAsync(Employee employee, Wizard wizard)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values.SelectMany(s => s.Errors).Select(s => s.ErrorMessage).ToArray());
                ErrorMessage = errors;
                _logger.LogWarning(errors);
                return RedirectToPage("./Index");
            }

            try
            {
                // Create employee
                var createdEmployee = await _employeeService.CreateEmployeeAsync(employee);

                // Add device
                if (!wizard.SkipDevice)
                {
                    await _employeeService.AddDeviceAsync(createdEmployee.Id, new string[] { wizard.DeviceId });

                    // Proximity Unlock
                    if (!wizard.SkipProximityUnlock)
                    {
                        await _proximityDeviceService.AddProximityDeviceAsync(wizard.WorkstationId, new string[] { wizard.DeviceId });
                    }

                    // Add workstation account
                    if (!wizard.WorkstationAccount.Skip)
                    {
                        await _employeeService.CreateWorkstationAccountAsync(wizard.WorkstationAccount, createdEmployee.Id, wizard.DeviceId);
                    }

                    _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(wizard.DeviceId);
                }

                SuccessMessage = $"Employee created.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<JsonResult> OnGetJsonWorkstationSharedAccountsAsync(string id)
        {
            if (id == null)
            {
                return new JsonResult(new WorkstationAccount());
            }

            var accountType = WorkstationAccountType.Local;
            var shared = await _sharedAccountService.Query().Where(d => d.Id == id).FirstOrDefaultAsync();
            var sharedType = shared.Login.Split('\\')[0];
            var sharedLogin = shared.Login.Split('\\')[1];
            switch (sharedType)
            {
                case ".":
                    accountType = WorkstationAccountType.Local;
                    break;
                case "@":
                    accountType = WorkstationAccountType.Microsoft;
                    break;
                default:
                    accountType = WorkstationAccountType.Domain;
                    break;
            }
            var personal = new WorkstationAccount()
            {
                Name = shared.Name,
                AccountType = accountType,
                Login = sharedLogin,
                Domain = sharedType,
                Password = shared.Password,
                ConfirmPassword = shared.Password
            };
            return new JsonResult(personal);
        }

        public async Task<IActionResult> OnGetEditEmployeeAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Employee = await _employeeService.GetEmployeeWithIncludeAsync(id);

            if (Employee == null)
            {
                _logger.LogWarning($"{nameof(Employee)} is null");
                return NotFound();
            }

            ViewData["CompanyId"] = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["DepartmentId"] = new SelectList(await _orgStructureService.DepartmentQuery().Where(d => d.CompanyId == Employee.Department.CompanyId).ToListAsync(), "Id", "Name");
            ViewData["PositionId"] = new SelectList(await _orgStructureService.PositionQuery().ToListAsync(), "Id", "Name");

            return Partial("_EditEmployee", this);
        }

        public async Task<IActionResult> OnPostEditEmployeeAsync(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values.SelectMany(s => s.Errors).Select(s => s.ErrorMessage).ToArray());
                ErrorMessage = errors;
                _logger.LogWarning(errors);
                return RedirectToPage("./Index");
            }

            try
            {
                await _employeeService.EditEmployeeAsync(employee);
                SuccessMessage = $"Employee updated.";
            }
            catch (Exception ex)
            {
                if (!await EmployeeExistsAsync(employee.Id))
                {
                    _logger.LogError("Employee dos not exists.");
                    return NotFound();
                }
                else
                {
                    ErrorMessage = ex.Message;
                    _logger.LogError(ex.Message);
                }
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetDeleteEmployeeAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Employee = await _employeeService
                .Query()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Employee == null)
            {
                _logger.LogWarning("Employee == null");
                return NotFound();
            }

            HasForeignKey = _deviceService
                .Query()
                .Where(x => x.EmployeeId == id)
                .Any();

            return Partial("_DeleteEmployee", this);
        }

        public async Task<IActionResult> OnPostDeleteEmployeeAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            try
            {
                await _employeeService.DeleteEmployeeAsync(id);
                SuccessMessage = $"Employee deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        private async Task<bool> EmployeeExistsAsync(string id)
        {
            return await _employeeService.ExistAsync(e => e.Id == id);
        }

        #endregion

        #region Company

        public IActionResult OnGetCreateCompany()
        {
            return Partial("_CreateCompany", this);
        }

        public async Task<IActionResult> OnPostCreateCompanyAsync(Company company)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values.SelectMany(s => s.Errors).Select(s => s.ErrorMessage).ToArray());
                _logger.LogWarning(errors);
                return new ContentResult() { Content = "error" };
            }

            try
            {
                await _orgStructureService.CreateCompanyAsync(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return new ContentResult() { Content = company.Name };
        }

        public async Task<JsonResult> OnGetJsonCompanyAsync()
        {
            return new JsonResult(await _orgStructureService.CompanyQuery().OrderBy(c => c.Name).ToListAsync());
        }

        #endregion

        #region Department

        public IActionResult OnGetCreateDepartment(string id)
        {
            ViewData["CompanyId"] = id;
            return Partial("_CreateDepartment", this);
        }

        public async Task<IActionResult> OnPostCreateDepartmentAsync(Department department)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values.SelectMany(s => s.Errors).Select(s => s.ErrorMessage).ToArray());
                _logger.LogWarning(errors);
                return new ContentResult() { Content = "error" };
            }

            try
            {
                await _orgStructureService.CreateDepartmentAsync(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return new JsonResult(new { department = department.Name, company = department.CompanyId });
        }

        public async Task<JsonResult> OnGetJsonDepartmentAsync(string id)
        {
            return new JsonResult(await _orgStructureService.DepartmentQuery().Where(d => d.CompanyId == id).OrderBy(d => d.Name).ToListAsync());
        }

        #endregion

        #region Position

        public IActionResult OnGetCreatePosition()
        {
            return Partial("_CreatePosition", this);
        }

        public async Task<IActionResult> OnPostCreatePositionAsync(Position position)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values.SelectMany(s => s.Errors).Select(s => s.ErrorMessage).ToArray());
                _logger.LogWarning(errors);
                return new ContentResult() { Content = "error" };
            }

            try
            {
                await _orgStructureService.CreatePositionAsync(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return new ContentResult() { Content = position.Name };
        }

        public async Task<JsonResult> OnGetJsonPositionAsync()
        {
            return new JsonResult(await _orgStructureService.PositionQuery().OrderBy(c => c.Name).ToListAsync());
        }

        #endregion
    }
}