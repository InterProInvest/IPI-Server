﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public class ChangePasswordModel : PageModel
    {
        public IActionResult OnGet()
        {
            return LocalRedirect("/RedirectToManage/ChangePassword");
        }

        public IActionResult OnPost()
        {
            return LocalRedirect("/RedirectToManage/ChangePassword");
        }
    }
}