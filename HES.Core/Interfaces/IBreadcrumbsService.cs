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
        Task SetGroups();
        public Task SetSharedAccounts();
        Task SetGroupDetails(string name);
        Task SetLicenseOrders();
        Task SetParameters();
    }
}