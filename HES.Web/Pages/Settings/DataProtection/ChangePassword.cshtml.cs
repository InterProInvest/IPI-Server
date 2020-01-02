using HES.Core.Interfaces;
using HES.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace HES.Web
{
    public class ChangePasswordModel : PageModel
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
            [Display(Name = "Old Password")]
            public string OldPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            public string NewPassword { get; set; }
        }

        public ChangePasswordModel(IDataProtectionService dataProtectionService)
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
                await _dataProtectionService.ChangeProtectionPasswordAsync(Input.OldPassword, Input.NewPassword);
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