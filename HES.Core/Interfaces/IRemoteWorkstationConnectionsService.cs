﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HES.Core.Entities;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Remote;

namespace HES.Core.Interfaces
{
    public interface IRemoteWorkstationConnectionsService : IDisposable
    {
        void StartUpdateRemoteDevice(IList<string> vaultIds);
        void StartUpdateRemoteDevice(string vaultId);
        Task LockAllWorkstationsAsync(string userEmail);
        Task UnlockAllWorkstationsAsync(string userEmail);
        Task UpdateRemoteDeviceAsync(string vaultId, string workstationId, bool primaryAccountOnly);
        Task RegisterWorkstationInfoAsync(IRemoteAppConnection remoteAppConnection, WorkstationInfoDto workstationInfo);
        Task OnAppHubDisconnectedAsync(string workstationId);
        Task UpdateProximitySettingsAsync(string workstationId, IReadOnlyList<DeviceProximitySettingsDto> proximitySettings);
        Task UpdateRfidStateAsync(string workstationId, bool isEnabled);
        Task UpdateWorkstationApprovedAsync(string workstationId, bool isApproved);
    }
}
