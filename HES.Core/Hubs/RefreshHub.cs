using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace HES.Core.Hubs
{
    public class RefreshHub : Hub
    {
        public async Task UpdatePage(string page, string entityId)
        {
            await Clients.All.SendAsync(page, entityId);
        }

        public async Task UpdateAllExcept(string page, string connectionId)
        {
            await Clients.AllExcept(connectionId).SendAsync(page);
        }
    }
}
