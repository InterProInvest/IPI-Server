﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Workstation;

namespace HES.Core.Interfaces
{
    public interface IRemoteWorkstationConnectionsService
    {
        void StartUpdateRemoteDevice(IList<string> devicesId);
        void StartUpdateRemoteDevice(string deviceId);
        Task UpdateRemoteDeviceAsync(string deviceId, string workstationId);
        Task RegisterWorkstationInfo(IRemoteAppConnection remoteAppConnection, WorkstationInfo workstationInfo);
        Task OnAppHubDisconnected(string workstationId);
    }
}
