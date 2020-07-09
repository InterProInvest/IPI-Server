using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.OrgStructure
{
    public class IndexModel : PageModel
    {
        private readonly IOrgStructureService _orgStructureService;
        private readonly IWorkstationService _workstationService;
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<IndexModel> _logger;

        public IList<Company> Companies { get; set; }
        public IList<Department> Departments { get; set; }

        public Company Company { get; set; }
        public Department Department { get; set; }
        public bool HasForeignKey { get; set; }
        public bool HasForeignKeyWorkstation { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }
        [ViewData]
        public SelectList CompanyId { get; set; }

        public IndexModel(IOrgStructureService orgStructureService,
                          IWorkstationService workstationService,
                          IEmployeeService employeeService,
                          ILogger<IndexModel> logger)
        {
            _orgStructureService = orgStructureService;
            _workstationService = workstationService;
            _employeeService = employeeService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            //Companies = await _orgStructureService.GetCompaniesAsync();
            //Departments = await _orgStructureService.GetDepartmentsAsync();
        }

        #region Company

        //public async Task<JsonResult> OnGetJsonCompanyAsync()
        //{
        //    return new JsonResult(await _orgStructureService.CompanyQuery().OrderBy(c => c.Name).ToListAsync());
        //}

        //public IActionResult OnGetCreateCompany()
        //{
        //    return Partial("_CreateCompany", this);
        //}

        //public async Task<IActionResult> OnPostCreateCompanyAsync(Company company)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        ErrorMessage = Validation.GetModelStateErrors(ModelState);
        //        _logger.LogError(ErrorMessage);
        //        return RedirectToPage("./Index");
        //    }

        //    try
        //    {
        //        await _orgStructureService.CreateCompanyAsync(company);
        //        SuccessMessage = $"Company created.";
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        ErrorMessage = ex.Message;
        //    }

        //    return RedirectToPage("./Index");
        //}

        //public async Task<IActionResult> OnGetEditCompanyAsync(string id)
        //{
        //    if (id == null)
        //    {
        //        _logger.LogWarning($"{nameof(id)} is null");
        //        return NotFound();
        //    }

        //    Company = await _orgStructureService.GetCompanyByIdAsync(id);

        //    if (Company == null)
        //    {
        //        _logger.LogWarning($"{nameof(Company)} is null");
        //        return NotFound();
        //    }

        //    return Partial("_EditCompany", this);
        //}

        //public async Task<IActionResult> OnPostEditCompanyAsync(Company company)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        ErrorMessage = Validation.GetModelStateErrors(ModelState);
        //        _logger.LogError(ErrorMessage);
        //        return RedirectToPage("./Index");
        //    }

        //    try
        //    {
        //        await _orgStructureService.EditCompanyAsync(company);
        //        SuccessMessage = $"Company updated.";
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        ErrorMessage = ex.Message;
        //    }

        //    return RedirectToPage("./Index");
        //}

        //public async Task<IActionResult> OnGetDeleteCompanyAsync(string id)
        //{
        //    if (id == null)
        //    {
        //        _logger.LogWarning($"{nameof(id)} is null");
        //        return NotFound();
        //    }

        //    Company = await _orgStructureService.GetCompanyByIdAsync(id);

        //    if (Company == null)
        //    {
        //        _logger.LogWarning($"{nameof(Company)} is null");
        //        return NotFound();
        //    }

        //    HasForeignKey = await _orgStructureService.DepartmentQuery().AnyAsync(x => x.CompanyId == id);

        //    return Partial("_DeleteCompany", this);
        //}

        //public async Task<IActionResult> OnPostDeleteCompanyAsync(string id)
        //{
        //    if (id == null)
        //    {
        //        _logger.LogWarning($"{nameof(id)} is null");
        //        return NotFound();
        //    }

        //    try
        //    {
        //        await _orgStructureService.DeleteCompanyAsync(id);
        //        SuccessMessage = $"Company deleted.";
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        ErrorMessage = ex.Message;
        //    }

        //    return RedirectToPage("./Index");
        //}

        #endregion

        #region Department

        public async Task<IActionResult> OnGetCreateDepartment()
        {
            CompanyId = new SelectList(await _orgStructureService.GetCompaniesAsync(), "Id", "Name");
            return Partial("_CreateDepartment", this);
        }

        public async Task<IActionResult> OnPostCreateDepartmentAsync(Department department)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = Validation.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Index");
            }

            try
            {
                await _orgStructureService.CreateDepartmentAsync(department);
                SuccessMessage = $"Department created.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditDepartmentAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Department = await _orgStructureService.GetDepartmentByIdAsync(id);

            if (Department == null)
            {
                _logger.LogWarning($"{nameof(Department)} is null");
                return NotFound();
            }

            CompanyId = new SelectList(await _orgStructureService.GetCompaniesAsync(), "Id", "Name");
            return Partial("_EditDepartment", this);
        }

        public async Task<IActionResult> OnPostEditDepartmentAsync(Department department)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = Validation.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Index");
            }

            try
            {
                await _orgStructureService.EditDepartmentAsync(department);
                SuccessMessage = $"Department updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetDeleteDepartmentAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Department = await _orgStructureService.GetDepartmentByIdAsync(id);

            if (Department == null)
            {
                _logger.LogWarning($"{nameof(Department)} is null");
                return NotFound();
            }

            HasForeignKey = await _employeeService.EmployeeQuery().AnyAsync(x => x.DepartmentId == id);
            HasForeignKeyWorkstation = await _workstationService.WorkstationQuery().AnyAsync(x => x.DepartmentId == id);

            return Partial("_DeleteDepartment", this);
        }

        public async Task<IActionResult> OnPostDeleteDepartmentAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            try
            {
                await _orgStructureService.DeleteDepartmentAsync(id);
                SuccessMessage = $"Department deleted.";
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
            return new JsonResult(await _orgStructureService.DepartmentQuery().Where(d => d.CompanyId == id).OrderBy(d => d.Name).ToListAsync());
        }

        #endregion
    }
}