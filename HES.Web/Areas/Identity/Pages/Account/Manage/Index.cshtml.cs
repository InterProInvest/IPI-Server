using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            return LocalRedirect("/RedirectToManage");
        }

        public IActionResult OnPost()
        {
            return LocalRedirect("/RedirectToManage");          
        }
    }
}