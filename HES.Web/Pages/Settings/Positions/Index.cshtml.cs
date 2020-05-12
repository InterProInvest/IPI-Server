using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Positions
{
    public class IndexModel : PageModel
    {
        private readonly IOrgStructureService _orgStructureService;
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<IndexModel> _logger;

        public IList<Position> Positions { get; set; }
        public bool HasForeignKey { get; set; }

        [BindProperty]
        public Position Position { get; set; }
        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(IOrgStructureService orgStructureService, IEmployeeService employeeService, ILogger<IndexModel> logger)
        {
            _orgStructureService = orgStructureService;
            _employeeService = employeeService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            Positions = await _orgStructureService.GetPositionsAsync();
        }

        #region Position

        public IActionResult OnGetCreatePosition()
        {
            return Partial("_CreatePosition", this);
        }

        public async Task<IActionResult> OnPostCreatePositionAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = Validation.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Index");
            }

            try
            {
                await _orgStructureService.CreatePositionAsync(Position);
                SuccessMessage = $"Position created.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditPositionAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Position = await _orgStructureService.GetPositionByIdAsync(id);

            if (Position == null)
            {
                _logger.LogWarning($"{nameof(Position)} is null");
                return NotFound();
            }

            return Partial("_EditPosition", this);
        }

        public async Task<IActionResult> OnPostEditPositionAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = Validation.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Index");
            }

            try
            {
                await _orgStructureService.EditPositionAsync(Position);
                SuccessMessage = $"Position updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetDeletePositionAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Position = await _orgStructureService.GetPositionByIdAsync(id);

            if (Position == null)
            {
                _logger.LogWarning($"{nameof(Position)} is null");
                return NotFound();
            }

            HasForeignKey = await _employeeService.EmployeeQuery().AnyAsync(x => x.PositionId == id);

            return Partial("_DeletePosition", this);
        }

        public async Task<IActionResult> OnPostDeletePositionAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            try
            {
                await _orgStructureService.DeletePositionAsync(id);
                SuccessMessage = $"Position deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<JsonResult> OnGetJsonPositionAsync()
        {
            return new JsonResult(await _orgStructureService.PositionQuery().OrderBy(c => c.Name).ToListAsync());
        }

        #endregion
    }
}