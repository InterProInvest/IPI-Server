using HES.Core.Models.Web.Dashboard;
using Microsoft.AspNetCore.Components;

namespace HES.Web.Pages.Dashboard
{
    public partial class Card : ComponentBase
    {
        [Parameter] public DashboardCard DashboardCard { get; set; }
    }
}