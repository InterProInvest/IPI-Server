using HES.Core.Models.Web.Breadcrumb;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IBreadcrumbsService
    {
        Task GetBreadcrumbs(out List<Breadcrumb> items);
        Task SetEmployees();
        Task SetEmployeeDetails(string name);
        Task SetHardwareVaults();
        Task SetGroups();
        Task SetGroupDetails(string name);
        Task SetParameters();
    }
}