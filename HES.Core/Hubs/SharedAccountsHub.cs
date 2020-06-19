using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace HES.Core.Hubs
{
    public class SharedAccountsHub : Hub
    {
        public async Task UpdatePage(string connectionId)
        {
            await Clients.All.SendAsync("PageUpdated", connectionId);
        }
    }
}
