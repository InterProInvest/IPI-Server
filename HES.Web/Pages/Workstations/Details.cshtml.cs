using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public class DetailsModel : PageModel
    {
        private readonly IWorkstationService _workstationService;
        public Workstation Workstation { get; set; }
        public string WorkstationId { get; set; }

        public DetailsModel(IWorkstationService workstationService)
        {
            _workstationService = workstationService;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Workstation = await _workstationService.GetWorkstationByIdAsync(id);

            if (Workstation == null)
            {
                return NotFound();
            }

            WorkstationId = id;

            return Page();
        }
    }
}