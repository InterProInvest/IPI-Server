using HES.Core.Entities;
using HES.Core.Models.Web.AppSettings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ISoftwareVaultService
    {
        Task CreateAndSendInvitationAsync(Employee employee, Server server, DateTime validTo);
    }
}
