using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HES.Web.Areas.Identity.Pages.Account.External
{
    public class ShowRecoveryCodesModel : PageModel
    {
        [TempData]
        public string[] RecoveryCodes { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (RecoveryCodes == null || RecoveryCodes.Length == 0)
            {
                return RedirectToPage("./TwoFactorAuthentication");
            }

            return Page();
        }
    }
}