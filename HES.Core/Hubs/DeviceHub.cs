using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Remote;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

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

        string GetWorkstationId()
        {
            if (Context.Items.TryGetValue("WorkstationId", out object workstationId))
                return (string)workstationId;
            else
            {
                _logger.LogCritical("DeviceHub does not contain WorkstationId!");
                throw new Exception("DeviceHub does not contain WorkstationId!");
            }
        }

        string GetDeviceId()
        {
            if (Context.Items.TryGetValue("DeviceId", out object deviceId))
                return (string)deviceId;
            else
            {
                _logger.LogCritical("DeviceHub does not contain DeviceId!");
                throw new Exception("DeviceHub does not contain DeviceId!");
            }
        }

        // HUB connection is connected
        public override async Task OnConnectedAsync()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                string deviceId = httpContext.Request.Headers["DeviceId"].ToString();
                string workstationId = httpContext.Request.Headers["WorkstationId"].ToString();
                Debug.WriteLine($"!!!!!!!!!!!!!!!!!!!!! OnConnectedAsync {deviceId}:{workstationId}");

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
                Debug.WriteLine($"!!!!!!!!!!!!!!!!!!!!! OnDisconnectedAsync");

                _remoteDeviceConnectionsService.OnDeviceHubDisconnected(GetDeviceId(), GetWorkstationId());
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "DeviceHub.OnDisconnectedAsync error");
            }
            return base.OnDisconnectedAsync(exception);
        }

        // Gets a device from the context
        RemoteDevice GetDevice()
        {
            var remoteDevice = _remoteDeviceConnectionsService.FindRemoteDevice(GetDeviceId(), GetWorkstationId());

            if (remoteDevice == null)
                throw new Exception($"Cannot find remote device in the DeviceHub");

            return remoteDevice;
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
                Debug.WriteLine(ex.Message);
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
                Debug.WriteLine(ex.Message);
                throw new HubException(ex.Message);
            }
            return Task.CompletedTask;
        }
    }
}