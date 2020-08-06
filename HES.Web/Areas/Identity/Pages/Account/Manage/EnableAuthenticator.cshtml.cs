using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public class EnableAuthenticatorModel : PageModel
    {
        public IActionResult OnGet()
        {
            return LocalRedirect("/RedirectToManage/EnableAuthenticator");
        }

        public IActionResult OnPost()
        {
            return LocalRedirect("/RedirectToManage/EnableAuthenticator");
        }
    }
}