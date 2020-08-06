using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public class Disable2faModel : PageModel
    {
        public IActionResult OnGet()
        {
            return LocalRedirect("/RedirectToManage/Disable2fa");
        }

        public IActionResult OnPost()
        {
            return LocalRedirect("/RedirectToManage/Disable2fa");
        }
    }
}