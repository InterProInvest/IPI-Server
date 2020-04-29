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

namespace HES.Web.Pages.Workstations
{
    public class IndexModel : PageModel
    {
        private readonly IWorkstationService _workstationService;
        private readonly IOrgStructureService _orgStructureService;
        private readonly ILogger<IndexModel> _logger;

        public IList<Workstation> Workstations { get; set; }
        public Workstation Workstation { get; set; }
        public WorkstationFilter WorkstationFilter { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }
        [ViewData]
        public SelectList CompanyIdList { get; set; }
        [ViewData]
        public SelectList DepartmentIdList { get; set; }

        public IndexModel(IWorkstationService workstationService, IOrgStructureService orgStructureService, ILogger<IndexModel> logger)
        {
            _workstationService = workstationService;
            _orgStructureService = orgStructureService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            Workstations = await _workstationService.GetWorkstationsAsync();

            ViewData["Companies"] = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["ProximityDevicesCount"] = new SelectList(Workstations.Select(s => s.WorkstationProximityVaults.Count()).Distinct().OrderBy(f => f).ToDictionary(t => t, t => t), "Key", "Value");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task OnGetNotApprovedAsync()
        {
            Workstations = await _workstationService
                .WorkstationQuery()
                .Include(w => w.WorkstationProximityVaults)
                .Include(c => c.Department.Company)
                .Where(w => w.Approved == false)
                .ToListAsync();

            ViewData["Companies"] = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["ProximityDevicesCount"] = new SelectList(Workstations.Select(s => s.WorkstationProximityVaults.Count()).Distinct().OrderBy(f => f).ToDictionary(t => t, t => t), "Key", "Value");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task OnGetOnlineAsync()
        {
            var allWorkstations = await _workstationService.GetWorkstationsAsync();

            Workstations = allWorkstations.Where(w => w.IsOnline == true).ToList();

            ViewData["Companies"] = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["ProximityDevicesCount"] = new SelectList(Workstations.Select(s => s.WorkstationProximityVaults.Count()).Distinct().OrderBy(f => f).ToDictionary(t => t, t => t), "Key", "Value");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task<IActionResult> OnPostFilterWorkstationsAsync(WorkstationFilter workstationFilter)
        {
            Workstations = await _workstationService.GetFilteredWorkstationsAsync(workstationFilter);

            return Partial("_WorkstationsTable", this);
        }

        public async Task<JsonResult> OnGetJsonDepartmentAsync(string id)
        {
            return new JsonResult(await _orgStructureService.DepartmentQuery().Where(d => d.CompanyId == id).ToListAsync());
        }

        public async Task<IActionResult> OnGetEditWorkstationAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Workstation = await _workstationService.GetWorkstationByIdAsync(id);

            if (Workstation == null)
            {
                _logger.LogWarning($"{nameof(Workstation)} is null");
                return NotFound();
            }

            var companies = await _orgStructureService.CompanyQuery().ToListAsync();
            List<Department> departments;
            if (Workstation.DepartmentId == null)
            {
                departments = await _orgStructureService.DepartmentQuery().Where(d => d.CompanyId == companies.FirstOrDefault().Id).ToListAsync();
            }
            else
            {
                departments = await _orgStructureService.DepartmentQuery().Where(d => d.CompanyId == Workstation.Department.CompanyId).ToListAsync();
            }

            CompanyIdList = new SelectList(companies, "Id", "Name");
            DepartmentIdList = new SelectList(departments, "Id", "Name");

            return Partial("_EditWorkstation", this);
        }

        public async Task<IActionResult> OnPostEditWorkstationAsync(Workstation workstation)
        {
            if (workstation == null)
            {
                _logger.LogWarning($"{nameof(workstation)} is null");
                return RedirectToPage("./Index");
            }

            try
            {
                await _workstationService.EditWorkstationAsync(workstation);
                await _workstationService.UpdateRfidStateAsync(workstation.Id);
                SuccessMessage = $"Workstation updated.";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetApproveWorkstationAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Workstation = await _workstationService.GetWorkstationByIdAsync(id);

            if (Workstation == null)
            {
                _logger.LogWarning($"{nameof(Workstation)} is null");
                return NotFound();
            }

            CompanyIdList = new SelectList(await _orgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");

            return Partial("_ApproveWorkstation", this);
        }

        public async Task<IActionResult> OnPostApproveWorkstationAsync(Workstation workstation)
        {
            if (workstation == null)
            {
                _logger.LogWarning($"{nameof(workstation)} is null");
                return RedirectToPage("./Index");
            }
            try
            {
                await _workstationService.ApproveWorkstationAsync(workstation);
                await _workstationService.UpdateRfidStateAsync(workstation.Id);
                SuccessMessage = $"Workstation approved.";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetUnapproveWorkstationAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Workstation = await _workstationService.GetWorkstationByIdAsync(id);

            if (Workstation == null)
            {
                _logger.LogWarning($"{nameof(Workstation)} is null");
                return NotFound();
            }

            return Partial("_UnapproveWorkstation", this);
        }

        public async Task<IActionResult> OnPostUnapproveWorkstationAsync(string workstationId)
        {
            if (workstationId == null)
            {
                _logger.LogWarning($"{nameof(workstationId)} is null");
                return RedirectToPage("./Index");
            }
            try
            {
                await _workstationService.UnapproveWorkstationAsync(workstationId);
                SuccessMessage = $"Workstation unapproved.";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Index");
        }
    }
}