using HES.Core.Models.Web.Breadcrumb;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IBreadcrumbsService
    {
        Task GetBreadcrumbs(out List<Breadcrumb> items);
        Task SetDashboard();
        Task SetEmployees();
        Task SetEmployeeDetails(string name);
        Task SetHardwareVaults();
        Task SetTemplates();
        Task SetDataProtection();
        Task SetGroups();
        Task SetAdministrators();
        Task SetHardwareVaultProfiles();
        public Task SetSharedAccounts();
        Task SetGroupDetails(string name);
        Task SetLicenseOrders();
        Task SetParameters();
        Task SetOrgStructure();
        Task SetWorkstations();
        Task SetWorkstationDetails(string name);
    }
}