using HES.Core.Interfaces;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.Client.Requests;
using Hideez.SDK.Communication.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Core.Hubs
{
    public class SoftwareVaultHub : Hub
    {
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly ILogger<SoftwareVaultHub> _logger;

        public SoftwareVaultHub(IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService, 
                                ILogger<SoftwareVaultHub> logger)
        {
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        // Incomming request
        public async Task<HesResponse> UnlockWorkstation(string workstationId, string unlockToken, string login, string password)
        {
            try
            {
                return await _remoteWorkstationConnectionsService.RequestWorkstationUnlockAsync(workstationId, unlockToken, login, password);
            }
            catch (Exception ex)
            {
                return new HesResponse(ex);
            }
        }
    }
}
