using HES.Core.Entities;
using HES.Core.Models.Web.AppSettings;
using HES.Core.Models.Web.SoftwareVault;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ISoftwareVaultService
    {
        IQueryable<SoftwareVault> SoftwareVaultQuery();
        Task<List<SoftwareVault>> GetSoftwareVaultsAsync(int skip, int take, string sortColumn, ListSortDirection sortDirection, string searchText, SoftwareVaultFilter filter);
        Task<int> GetVaultsCountAsync(string searchText, SoftwareVaultFilter filter);
        Task<List<SoftwareVaultInvitation>> GetSoftwareVaultInvitationsAsync();
        Task CreateAndSendInvitationAsync(Employee employee, Server server, DateTime validTo);
    }
}