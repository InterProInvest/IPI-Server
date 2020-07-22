using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSessions
{
    public class IndexModel : PageModel
    {
        public async Task OnGetNonHideezUnlockAsync()
        {
            //WorkstationSessions = await _workstationAuditService
            //    .SessionQuery()
            //    .Include(w => w.Workstation)
            //    .Include(w => w.HardwareVault)
            //    .Include(w => w.Employee)
            //    .Include(w => w.Department.Company)
            //    .Include(w => w.Account)
            //    .Where(w => w.StartDate >= DateTime.UtcNow.AddDays(-1) && w.UnlockedBy == Hideez.SDK.Communication.SessionSwitchSubject.NonHideez)
            //    .OrderByDescending(w => w.StartDate)
            //    .Take(500)
            //    .ToListAsync();
        }

        public async Task OnGetLongOpenSessionAsync()
        {
            //WorkstationSessions = await _workstationAuditService
            //    .SessionQuery()
            //    .Include(w => w.Workstation)
            //    .Include(w => w.HardwareVault)
            //    .Include(w => w.Employee)
            //    .Include(w => w.Department.Company)
            //    .Include(w => w.Account)
            //    .Where(w => w.StartDate <= DateTime.UtcNow.AddHours(-12) && w.EndDate == null)
            //    .OrderByDescending(w => w.StartDate)
            //    .Take(500)
            //    .ToListAsync();
        }

        public async Task OnGetOpenedSessionsAsync()
        {
            //WorkstationSessions = await _workstationAuditService
            //    .SessionQuery()
            //    .Include(w => w.Workstation)
            //    .Include(w => w.HardwareVault)
            //    .Include(w => w.Employee)
            //    .Include(w => w.Department.Company)
            //    .Include(w => w.Account)
            //    .Where(w => w.EndDate == null)
            //    .OrderByDescending(w => w.StartDate)
            //    .Take(500)
            //    .ToListAsync();
        }
    }
}