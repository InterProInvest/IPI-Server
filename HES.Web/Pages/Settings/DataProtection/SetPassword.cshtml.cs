using HES.Core.Interfaces;
using HES.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace HES.Web
{
    public class SetPasswordModel : PageModel
    {
        private readonly IDataProtectionService _dataProtectionService;

        [TempData]
        public string ErrorMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public SetPasswordModel(IDataProtectionService dataProtectionService)
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

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await _dataProtectionService.EnableProtectionAsync(Input.Password);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
            return LocalRedirect(Url.Content("~/"));
        }
    }
}