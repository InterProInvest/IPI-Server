using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace HES.Web.Pages.Employees
{
    public class DetailsModel : PageModel
    {
        private readonly ILogger<DetailsModel> _logger;
        public string EmployeeId { get; set; }

        public DetailsModel(ILogger<DetailsModel> logger)
        {         
            _logger = logger;
        }

        public IActionResult OnGetAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            EmployeeId = id;
            return Page();
        }
    }
}