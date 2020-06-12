using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace HES.Core.Hubs
{
    public class EmployeeDetailsHub : Hub
    {
        public async Task RefreshPage(string employeeId, string connectionId)
        {
            await Clients.All.SendAsync("UpdatePage", employeeId, connectionId);
        }
    }
}
