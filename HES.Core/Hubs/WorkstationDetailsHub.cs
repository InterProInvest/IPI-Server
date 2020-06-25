using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace HES.Core.Hubs
{
    public class WorkstationDetailsHub : Hub
    {
        public async Task UpdatePage(string workstationId, string connectionId)
        {
            await Clients.All.SendAsync("PageUpdated", workstationId, connectionId);
        }
    }
}
