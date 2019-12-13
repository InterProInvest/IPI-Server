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
    public class IndexModel : PageModel
    {
        private readonly HES.Infrastructure.ApplicationDbContext _context;
        public IList<LicenseOrder> LicenseOrder { get; set; }

        public IndexModel(HES.Infrastructure.ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            LicenseOrder = await _context.LicenseOrders.ToListAsync();
        }
    }
}
