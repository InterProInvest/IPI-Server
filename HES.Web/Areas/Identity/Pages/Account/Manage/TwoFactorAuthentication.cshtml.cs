using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public class TwoFactorAuthenticationModel : PageModel
    {
        public IActionResult OnGet()
        {
            return LocalRedirect("/RedirectToManage/TwoFactorAuthentication");
        }

        public IActionResult OnPost()
        {
            return LocalRedirect("/RedirectToManage/TwoFactorAuthentication");
        }        
    }
}