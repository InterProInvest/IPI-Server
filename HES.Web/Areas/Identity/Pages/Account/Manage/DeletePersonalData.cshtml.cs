using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        public IActionResult OnGet()
        {
            return LocalRedirect("/RedirectToManage/DeletePersonalData");
        }

        public IActionResult OnPost()
        {
            return LocalRedirect("/RedirectToManage/DeletePersonalData");
        }
    }
}