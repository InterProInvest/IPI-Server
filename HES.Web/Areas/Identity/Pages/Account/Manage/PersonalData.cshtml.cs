﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public class PersonalDataModel : PageModel
    {
        public IActionResult OnGet()
        {
            return LocalRedirect("/RedirectToManage/PersonalData");
        }

        public IActionResult OnPost()
        {
            return LocalRedirect("/RedirectToManage/PersonalData");
        }
    }
}