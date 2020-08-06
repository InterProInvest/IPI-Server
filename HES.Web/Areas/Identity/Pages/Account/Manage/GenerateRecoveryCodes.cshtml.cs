using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public class GenerateRecoveryCodesModel : PageModel
    {
        public IActionResult OnGet()
        {
            return LocalRedirect("/RedirectToManage/GenerateRecoveryCodes");
        }

        public IActionResult OnPost()
        {
            return LocalRedirect("/RedirectToManage/GenerateRecoveryCodes");
        }
    }
}