using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Templates = await _templateService.GetTemplatesAsync();
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
                ErrorMessage = ValidationHepler.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
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
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Template = await _templateService.GetByIdAsync(id);

            if (Template == null)
            {
                _logger.LogWarning($"{nameof(Template)} is null");
                return NotFound();
            }

            return Partial("_EditTemplate", this);
        }

        public async Task<IActionResult> OnPostEditTemplateAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = ValidationHepler.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
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
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Template = await _templateService.GetByIdAsync(id);

            if (Template == null)
            {
                _logger.LogWarning($"{nameof(Template)} is null");
                return NotFound();
            }

            return Partial("_DeleteTemplate", this);
        }

        public async Task<IActionResult> OnPostDeleteTemplateAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
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