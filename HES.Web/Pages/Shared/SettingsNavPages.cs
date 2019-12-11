using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace HES.Web.Pages
{
    public static class SettingsNavPages
    {
        public static string Administrators => "./Administrators/Index";

        public static string DataProtection => "./DataProtection/Index";

        public static string DeviceAccessProfiles => "./DeviceAccessProfiles/Index";

        public static string OrgStructure => "./OrgStructure/Index";

        public static string Positions => "./Positions/Index";

        public static string DeviceLicenses => "./DeviceLicenses/Index";

        public static string IdentityProvider => "./IdentityProvider/Index";


        public static string AdministratorsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Administrators);

        public static string DataProtectionNavClass(ViewContext viewContext) => PageNavClass(viewContext, DataProtection);

        public static string DeviceAccessProfilesNavClass(ViewContext viewContext) => PageNavClass(viewContext, DeviceAccessProfiles);

        public static string OrgStructureNavClass(ViewContext viewContext) => PageNavClass(viewContext, OrgStructure);

        public static string PositionsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Positions);

        public static string DeviceLicensesNavClass(ViewContext viewContext) => PageNavClass(viewContext, DeviceLicenses);

        public static string IdentityProviderNavClass(ViewContext viewContext) => PageNavClass(viewContext, IdentityProvider);


        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["SettingsPage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}