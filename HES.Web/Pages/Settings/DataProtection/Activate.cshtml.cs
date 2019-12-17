using HES.Core.Interfaces;
using HES.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace HES.Web
{
    public class ActivateModel : PageModel
    {
        private readonly IDataProtectionService _dataProtectionService;

        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            public string Password { get; set; }
        }
        
        public ActivateModel(IDataProtectionService dataProtectionService)
        {
            _dataProtectionService = dataProtectionService;
        }

        public IActionResult OnGetAsync()
        {
            var status = _dataProtectionService.Status();

            if (status == ProtectionStatus.On)
            {
                return LocalRedirect(Url.Content("~/"));
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            try
            {
                var result = await _dataProtectionService.ActivateProtectionAsync(Input.Password);
                if (!result)
                {
                    ModelState.AddModelError("Input.Password", "Invalid password.");
                    return Page();
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Input.Password", ex.Message);
                return Page();
            }
            return LocalRedirect(returnUrl);
        }
    }
}