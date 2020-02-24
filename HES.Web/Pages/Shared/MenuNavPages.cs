using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HES.Web.Pages
{
    public static class MenuNavPages
    {
        public static string Dashboard => "./Dashboard/Index";

        public static string Employees => "./Employees/Index";

        public static string Workstations => "./Workstations/Index";

        public static string Groups => "./Groups/Index";

        public static string SharedAccounts => "./SharedAccounts/Index";

        public static string Templates => "./Templates/Index";

        public static string Devices => "./Devices/Index";

        public static string Audit => "./Audit/Index";

        public static string Settings => "./Settings/Index";


        public static string DashboardNavClass(ViewContext viewContext) => PageNavClass(viewContext, Dashboard);

        public static string EmployeesNavClass(ViewContext viewContext) => PageNavClass(viewContext, Employees);

        public static string WorkstationsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Workstations);

        public static string GroupsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Groups);

        public static string SharedAccountsNavClass(ViewContext viewContext) => PageNavClass(viewContext, SharedAccounts);

        public static string TemplatesNavClass(ViewContext viewContext) => PageNavClass(viewContext, Templates);

        public static string DevicesNavClass(ViewContext viewContext) => PageNavClass(viewContext, Devices);

        public static string AuditNavClass(ViewContext viewContext) => PageNavClass(viewContext, Audit);

        public static string SettingsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Settings);


        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}