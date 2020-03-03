using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HES.Web.Pages.Groups
{
    public class DetailsModel : PageModel
    {
        public string Id { get; set; }

        public IActionResult OnGet(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Id = id;
            return Page();
        }
    }
}