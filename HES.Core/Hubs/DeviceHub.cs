using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Remote;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Core.Hubs
{
    public class DeviceHub : Hub<IRemoteCommands>
    {
        readonly IRemoteDeviceConnectionsService _remoteDeviceConnectionsService;
        readonly ILogger<DeviceHub> _logger;

        public DeviceHub(IRemoteDeviceConnectionsService remoteDeviceConnectionsService,
                         ILogger<DeviceHub> logger)
        {
            _remoteDeviceConnectionsService = remoteDeviceConnectionsService;
            _logger = logger;
        }

        private string GetWorkstationId()
        {
            if (Context.Items.TryGetValue("WorkstationId", out object workstationId))
                return (string)workstationId;
            else
            {
                throw new Exception("DeviceHub does not contain WorkstationId!");
            }
        }

        private string GetDeviceId()
        {
            if (Context.Items.TryGetValue("DeviceId", out object deviceId))
                return (string)deviceId;
            else
            {
                throw new Exception("DeviceHub does not contain DeviceId!");
            }
        }

        // Gets a device from the context
        private RemoteDevice GetDevice()
        {
            var remoteDevice = _remoteDeviceConnectionsService.FindRemoteDevice(GetDeviceId(), GetWorkstationId());

            if (remoteDevice == null)
                throw new Exception($"Cannot find remote device in the DeviceHub");

            return remoteDevice;
        }

        // HUB connection is connected
        public override async Task OnConnectedAsync()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                string deviceId = httpContext.Request.Headers["DeviceId"].ToString();
                string workstationId = httpContext.Request.Headers["WorkstationId"].ToString();

                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    _logger.LogCritical($"DeviceId cannot be empty");
                }
                else if (string.IsNullOrWhiteSpace(workstationId))
                {
                    _logger.LogCritical($"WorkstationId cannot be empty");
                }
                else
                {
                    Context.Items.Add("DeviceId", deviceId);
                    Context.Items.Add("WorkstationId", workstationId);

                    _remoteDeviceConnectionsService.OnDeviceHubConnected(deviceId, workstationId, Clients.Caller);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "DeviceHub.OnConnectedAsync error");
            }

            await base.OnConnectedAsync();
        }

        // HUB connection is disconnected (OnDeviceDisconnected received in AppHub)
        public override Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                _remoteDeviceConnectionsService.OnDeviceHubDisconnected(GetDeviceId(), GetWorkstationId());
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "DeviceHub.OnDisconnectedAsync error");
            }
            return base.OnDisconnectedAsync(exception);
        }

        // Incoming request
        public Task OnVerifyResponse(byte[] data, string error)
        {
            try
            {
                var device = GetDevice();
                device.OnVerifyResponse(data, error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new HubException(ex.Message);
            }
            return Task.CompletedTask;
        }

        // Incoming request
        public Task OnCommandResponse(byte[] data, string error)
        {
            try
            {
                var device = GetDevice();
                device.OnCommandResponse(data, error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new HubException(ex.Message);
            }
            return Task.CompletedTask;
        }
    }
}