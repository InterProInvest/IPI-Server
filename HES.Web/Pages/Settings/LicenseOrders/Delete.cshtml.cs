using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HES.Core.Entities;
using HES.Infrastructure;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public class DeleteModel : PageModel
    {
        private readonly HES.Infrastructure.ApplicationDbContext _context;

        public DeleteModel(HES.Infrastructure.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public LicenseOrder LicenseOrder { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            LicenseOrder = await _context.LicenseOrders.FirstOrDefaultAsync(m => m.Id == id);

            if (LicenseOrder == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            LicenseOrder = await _context.LicenseOrders.FindAsync(id);

            if (LicenseOrder != null)
            {
                _context.LicenseOrders.Remove(LicenseOrder);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
