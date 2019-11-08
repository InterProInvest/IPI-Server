using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Templates
{
    public class IndexModel : PageModel
    {
        private readonly ITemplateService _templateService;
        private readonly ILogger<IndexModel> _logger;

        public IList<Template> Templates { get; set; }

        [BindProperty]
        public Template Template { get; set; }
        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(ITemplateService templateService, ILogger<IndexModel> logger)
        {
            _templateService = templateService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            Templates = await _templateService.Query().ToListAsync();
        }

        #region Tempalate

        public IActionResult OnGetCreateTemplate()
        {
            return Partial("_CreateTemplate", this);
        }

        public async Task<IActionResult> OnPostCreateTemplateAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model is not valid");
                return RedirectToPage("./Index");
            }

            try
            {
                await _templateService.CreateTmplateAsync(Template);
                SuccessMessage = $"Template created.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditTemplateAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Template = await _templateService
                .Query()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Template == null)
            {
                _logger.LogWarning("Template == null");
                return NotFound();
            }

            return Partial("_EditTemplate", this);
        }

        public async Task<IActionResult> OnPostEditTemplateAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model is not valid");
                return RedirectToPage("./Index");
            }

            try
            {
                await _templateService.EditTemplateAsync(Template);
                SuccessMessage = $"Template updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetDeleteTemplateAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Template = await _templateService
                .Query()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Template == null)
            {
                _logger.LogWarning("Template == null");
                return NotFound();
            }

            return Partial("_DeleteTemplate", this);
        }

        public async Task<IActionResult> OnPostDeleteTemplateAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            try
            {
                await _templateService.DeleteTemplateAsync(id);
                SuccessMessage = $"Template deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        #endregion
    }
}