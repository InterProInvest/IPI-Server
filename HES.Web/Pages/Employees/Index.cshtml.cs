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
using HES.Core.Enums;

namespace HES.Web.Pages.Employees
{
    public class IndexModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IHardwareVaultService _deviceService;
        private readonly IWorkstationService _workstationService;
        private readonly IOrgStructureService _orgStructureService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly ISharedAccountService _sharedAccountService;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly ILogger<IndexModel> _logger;

        public IList<Employee> Employees { get; set; }
        public IList<HardwareVault> Devices { get; set; }
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
        [ViewData]
        public string CompanyId { get; set; }
        [ViewData]
        public SelectList CompanyIdList { get; set; }
        [ViewData]
        public SelectList DepartmentIdList { get; set; }
        [ViewData]
        public SelectList PositionIdList { get; set; }
        [ViewData]
        public SelectList DeviceIdList { get; set; }
        [ViewData]
        public SelectList WorkstationIdList { get; set; }
        [ViewData]
        public SelectList WorkstationAccountTypeList { get; set; }
        [ViewData]
        public SelectList WorkstationAccountsList { get; set; }


        public IndexModel(IEmployeeService employeeService,
                          IHardwareVaultService deviceService,
                          IWorkstationService workstationService,
                          IOrgStructureService orgStructureService,
                          IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                          ISharedAccountService sharedAccountService,
                          IDataProtectionService dataProtectionService,
                          ILogger<IndexModel> logger)
        {
            _employeeService = employeeService;
            _deviceService = deviceService;
            _workstationService = workstationService;
            _orgStructureService = orgStructureService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _sharedAccountService = sharedAccountService;
            _dataProtectionService = dataProtectionService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            Employees = await _employeeService.GetEmployeesAsync();

            ViewData["Companies"] = new SelectList(await _orgStructureService.CompanyQuery().OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["Positions"] = new SelectList(await _orgStructureService.PositionQuery().OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["DevicesCount"] = new SelectList(Employees.Select(s => s.HardwareVaults.Count()).Distinct().OrderBy(f => f).ToDictionary(t => t, t => t), "Key", "Value");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        #region Employee

        public async Task<IActionResult> OnPostFilterEmployeesAsync(EmployeeFilter employeeFilter)
        {
            Employees = await _employeeService.GetFilteredEmployeesAsync(employeeFilter);
            return Partial("_EmployeesTable", this);
        }

        public async Task<IActionResult> OnGetCreateEmployeeAsync()
        {
            CompanyIdList = new SelectList(await _orgStructureService.CompanyQuery().OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            PositionIdList = new SelectList(await _orgStructureService.PositionQuery().OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            DeviceIdList = new SelectList(await _deviceService.VaultQuery().Where(d => d.EmployeeId == null && d.Status == Core.Enums.VaultStatus.Ready).ToListAsync(), "Id", "Id");
            WorkstationIdList = new SelectList(await _workstationService.WorkstationQuery().ToListAsync(), "Id", "Name");
            WorkstationAccountTypeList = new SelectList(Enum.GetValues(typeof(WorkstationAccountType)).Cast<WorkstationAccountType>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            WorkstationAccountsList = new SelectList(await _sharedAccountService.Query().Where(s => s.Kind == AccountKind.Workstation && s.Deleted == false).OrderBy(c => c.Name).ToListAsync(), "Id", "Name");

            Devices = await _deviceService
               .VaultQuery()
               .Where(d => d.EmployeeId == null)
               .ToListAsync();

            Workstations = await _workstationService
                .WorkstationQuery()
                .ToListAsync();

            return Partial("_CreateEmployee", this);
        }

        public async Task<IActionResult> OnPostCreateEmployeeAsync(Employee employee, Wizard wizard)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = Validation.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Index");
            }

            try
            {
                // Create employee
                var createdEmployee = await _employeeService.CreateEmployeeAsync(employee);

                // Add device
                if (!wizard.SkipDevice)
                {
                    await _employeeService.AddHardwareVaultAsync(createdEmployee.Id, wizard.DeviceId);

                    // Proximity Unlock
                    if (!wizard.SkipProximityUnlock)
                    {
                        await _workstationService.AddProximityDevicesAsync(wizard.WorkstationId, new string[] { wizard.DeviceId });
                    }

                    // Add workstation account
                    if (!wizard.WorkstationAccount.Skip)
                    {
                        await _employeeService.CreateWorkstationAccountAsync(wizard.WorkstationAccount, createdEmployee.Id);
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
                Password = _dataProtectionService.Decrypt(shared.Password),
                ConfirmPassword = _dataProtectionService.Decrypt(shared.Password)
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

            Employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (Employee == null)
            {
                _logger.LogWarning($"{nameof(Employee)} is null");
                return NotFound();
            }

            CompanyIdList = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");
            if (Employee.Department?.CompanyId != null)
            {
                CompanyId = Employee.Department.CompanyId;
                DepartmentIdList = new SelectList(await _orgStructureService.DepartmentQuery().Where(d => d.CompanyId == Employee.Department.CompanyId).ToListAsync(), "Id", "Name");
            }
            PositionIdList = new SelectList(await _orgStructureService.PositionQuery().ToListAsync(), "Id", "Name");

            return Partial("_EditEmployee", this);
        }

        public async Task<IActionResult> OnPostEditEmployeeAsync(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = Validation.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
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
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (Employee == null)
            {
                _logger.LogWarning($"{nameof(Employee)} is null");
                return NotFound();
            }

            HasForeignKey = _deviceService
                .VaultQuery()
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
                var errorMessage = Validation.GetModelStateErrors(ModelState);
                _logger.LogError(errorMessage);
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
            CompanyId = id;
            return Partial("_CreateDepartment", this);
        }

        public async Task<IActionResult> OnPostCreateDepartmentAsync(Department department)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = Validation.GetModelStateErrors(ModelState);
                _logger.LogError(errorMessage);
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
                var errorMessage = Validation.GetModelStateErrors(ModelState);
                _logger.LogError(errorMessage);
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