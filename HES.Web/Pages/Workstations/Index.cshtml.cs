using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public class IndexModel : PageModel
    {
        public string DashboardFilter { get; set; }

        public void OnGet()
        {

        }

        public void OnGetNotApprovedAsync()
        {
            DashboardFilter = "NotApproved";
        }

        public void OnGetOnlineAsync()
        {
            DashboardFilter = "Online";
        }
    }
}