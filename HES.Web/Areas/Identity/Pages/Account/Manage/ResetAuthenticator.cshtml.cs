using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public class ResetAuthenticatorModel : PageModel
    {
        public IActionResult OnGet()
        {
            return LocalRedirect("/RedirectToManage/ResetAuthenticator");
        }

        public IActionResult OnPost()
        {
            return LocalRedirect("/RedirectToManage/ResetAuthenticator");
        }
    }
}