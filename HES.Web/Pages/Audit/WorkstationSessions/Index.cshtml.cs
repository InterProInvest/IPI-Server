using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HES.Web.Pages.Audit.WorkstationSessions
{
    public class IndexModel : PageModel
    {
        public string DashboardFilter { get; set; }

        public void OnGetNonHideezUnlock()
        {
            DashboardFilter = "NonHideezUnlock";
        }

        public void OnGetLongOpenSession()
        {
            DashboardFilter = "LongOpenSession";
        }

        public void OnGetOpenedSessions()
        {
            DashboardFilter = "OpenedSessions";
        }
    }
}