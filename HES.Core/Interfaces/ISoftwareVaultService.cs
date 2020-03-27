using HES.Core.Entities;
using HES.Core.Models.Web.AppSettings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ISoftwareVaultService
    {
        Task<List<SoftwareVault>> GetSoftwareVaultsAsync();
        Task<List<SoftwareVaultInvitation>> GetSoftwareVaultInvitationsAsync();
        Task CreateAndSendInvitationAsync(Employee employee, Server server, DateTime validTo);
    }
}