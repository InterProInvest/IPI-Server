using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace HES.Core.Hubs
{
    public class EmployeeDetailsHub : Hub
    {
        public async Task UpdatePage(string employeeId, string connectionId)
        {
            await Clients.All.SendAsync("PageUpdated", employeeId, connectionId);
        }

        public async Task SyncVault(string employeeId)
        {
            await Clients.All.SendAsync("VaultSynced", employeeId);
        }
    }
}