using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HES.Core.Entities;
using HES.Infrastructure;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public class EditModel : PageModel
    {
        private readonly HES.Infrastructure.ApplicationDbContext _context;

        public EditModel(HES.Infrastructure.ApplicationDbContext context)
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(LicenseOrder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LicenseOrderExists(LicenseOrder.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool LicenseOrderExists(string id)
        {
            return _context.LicenseOrders.Any(e => e.Id == id);
        }
    }
}
