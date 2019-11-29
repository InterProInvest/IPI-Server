using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Command;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Utils;
using Hideez.SDK.Communication.Workstation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HES.Core.Services
{
    public class RemoteWorkstationConnectionsService : IRemoteWorkstationConnectionsService
    {
        static readonly ConcurrentDictionary<string, IRemoteAppConnection> _workstationConnections
                    = new ConcurrentDictionary<string, IRemoteAppConnection>();

        static readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _devicesInProgress
            = new ConcurrentDictionary<string, TaskCompletionSource<bool>>();

        readonly IServiceProvider _services;
        readonly IRemoteTaskService _remoteTaskService;
        readonly IRemoteDeviceConnectionsService _remoteDeviceConnectionsService;
        readonly IEmployeeService _employeeService;
        readonly IWorkstationService _workstationService;
        readonly IDeviceService _deviceService;
        readonly IDataProtectionService _dataProtectionService;
        readonly IWorkstationAuditService _workstationAuditService;
        readonly ILogger<RemoteWorkstationConnectionsService> _logger;

        public RemoteWorkstationConnectionsService(IServiceProvider services,
                      IRemoteTaskService remoteTaskService,
                      IRemoteDeviceConnectionsService remoteDeviceConnectionsService,
                      IEmployeeService employeeService,
                      IWorkstationService workstationService,
                      IDeviceService deviceService,
                      IDataProtectionService dataProtectionService,
                      IWorkstationAuditService workstationAuditService,
                      ILogger<RemoteWorkstationConnectionsService> logger)
        {
            _services = services;
            _remoteTaskService = remoteTaskService;
            _remoteDeviceConnectionsService = remoteDeviceConnectionsService;
            _employeeService = employeeService;
            _workstationService = workstationService;
            _deviceService = deviceService;
            _dataProtectionService = dataProtectionService;
            _workstationAuditService = workstationAuditService;
            _logger = logger;
        }

        #region Device

        public void StartUpdateRemoteDevice(IList<string> devicesId)
        {
            foreach (var device in devicesId)
            {
                StartUpdateRemoteDevice(device);
            }
        }

        public void StartUpdateRemoteDevice(string deviceId)
        {
            if (deviceId == null)
                throw new ArgumentNullException(nameof(deviceId));

#pragma warning disable IDE0067 // Dispose objects before losing scope
            var scope = _services.CreateScope();
#pragma warning restore IDE0067 // Dispose objects before losing scope

            if (!RemoteDeviceConnectionsService.IsDeviceConnectedToHost(deviceId))
            {
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    var remoteWorkstationConnectionsService = scope.ServiceProvider
                            .GetRequiredService<IRemoteWorkstationConnectionsService>();

                    await remoteWorkstationConnectionsService.UpdateRemoteDeviceAsync(deviceId, workstationId: null, primaryAccountOnly: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[{deviceId}] {ex.Message}");
                }
                finally
                {
                    scope.Dispose();
                }
            });
        }

        public async Task UpdateRemoteDeviceAsync(string deviceId, string workstationId, bool primaryAccountOnly)
        {
            Debug.WriteLine($"!!!!!!!!!!!!! UpdateRemoteDeviceAsync start {deviceId}");
            if (deviceId == null)
                throw new ArgumentNullException(nameof(deviceId));

            var isNew = false;
            var tcs = _devicesInProgress.GetOrAdd(deviceId, (x) =>
            {
                isNew = true;
                return new TaskCompletionSource<bool>();
            });

            if (!isNew)
            {
                Debug.WriteLine($"!!!!!!!!!!!!! UpdateRemoteDeviceAsync already running {deviceId}");
                await tcs.Task;
                return;
            }

            bool masterKeyError = false;

            try
            {
                await UpdateRemoteDevice(deviceId, workstationId, primaryAccountOnly).TimeoutAfter(300_000);
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
                _logger.LogError($"[{deviceId}] {ex.Message}");

                if (ex is HideezException hex && hex.ErrorCode == HideezErrorCode.ERR_KEY_WRONG)
                    masterKeyError = true;

                throw;
            }
            finally
            {
                if (masterKeyError)
                {
                    try
                    {
                        await _employeeService.HandlingMasterPasswordErrorAsync(deviceId);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogCritical($"[{deviceId}] {ex.Message}");
                    }
                }
                _devicesInProgress.TryRemove(deviceId, out TaskCompletionSource<bool> _);
            }
        }

        private async Task<bool> UpdateRemoteDevice(string deviceId, string workstationId, bool primaryAccountOnly)
        {
            Debug.WriteLine($"!!!!!!!!!!!!! UpdateRemoteDevice {deviceId}");
            //todo
            //if (true) //conection not approved
            //throw new HideezException(HideezErrorCode.HesWorkstationNotApproved);

            var remoteDevice = await _remoteDeviceConnectionsService
                .ConnectDevice(deviceId, workstationId)
                .TimeoutAfter(30_000);

            if (remoteDevice == null)
                throw new HideezException(HideezErrorCode.HesFailedEstablishRemoteDeviceConnection);

            var device = await _deviceService
                .DeviceQuery()
                .Include(d => d.DeviceAccessProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == deviceId);

            if (device == null)
                throw new HideezException(HideezErrorCode.HesDeviceNotFound);

            // Getting device info
            await remoteDevice.Initialize();

            // unlocking the device 
            if (remoteDevice.AccessLevel.IsLocked)
            {
                // execute the UnlockPin task
                await _remoteTaskService.ExecuteRemoteTasks(deviceId, remoteDevice, TaskOperation.UnlockPin);
                await remoteDevice.RefreshDeviceInfo();
            }

            // try to wipe the device - this is the most priority task
            await _remoteTaskService.ExecuteRemoteTasks(deviceId, remoteDevice, TaskOperation.Wipe);

            // linking the device
            if (remoteDevice.AccessLevel.IsLinkRequired)
            {
                // execute the Link task
                await _remoteTaskService.ExecuteRemoteTasks(deviceId, remoteDevice, TaskOperation.Link);
                await remoteDevice.RefreshDeviceInfo();

                // refresh MasterPassword field
                device = await _deviceService
                    .DeviceQuery()
                    .Include(d => d.DeviceAccessProfile)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == deviceId);

                if (remoteDevice.AccessLevel.IsLinkRequired)
                {
                    // we have tried to link the device with no success. Just in case clearing all device's tasks and linkings
                    await _employeeService.HandlingMasterPasswordErrorAsync(deviceId);
                    throw new HideezException(HideezErrorCode.HesDeviceNotAssignedToAnyUser);
                }
            }

            // Access 
            if (device.DeviceAccessProfile == null)
                throw new HideezException(HideezErrorCode.HesEmptyDeviceAccessProfile);

            var accessParams = new AccessParams()
            {
                MasterKey_Bond = device.DeviceAccessProfile.MasterKeyBonding,
                MasterKey_Connect = device.DeviceAccessProfile.MasterKeyConnection,
                MasterKey_Channel = device.DeviceAccessProfile.MasterKeyNewChannel,
                MasterKey_Link = device.DeviceAccessProfile.MasterKeyConnection,

                Button_Bond = device.DeviceAccessProfile.ButtonBonding,
                Button_Connect = device.DeviceAccessProfile.ButtonConnection,
                Button_Channel = device.DeviceAccessProfile.ButtonNewChannel,
                Button_Link = device.DeviceAccessProfile.ButtonConnection,

                Pin_Bond = device.DeviceAccessProfile.PinBonding,
                Pin_Connect = device.DeviceAccessProfile.PinConnection,
                Pin_Channel = device.DeviceAccessProfile.PinNewChannel,
                Pin_Link = device.DeviceAccessProfile.PinConnection,

                PinMinLength = device.DeviceAccessProfile.PinLength,
                PinMaxTries = device.DeviceAccessProfile.PinTryCount,
                PinExpirationPeriod = device.DeviceAccessProfile.PinExpiration,
                ButtonExpirationPeriod = 0,
                MasterKeyExpirationPeriod = 0
            };

            if (string.IsNullOrWhiteSpace(device.MasterPassword))
                throw new HideezException(HideezErrorCode.HesEmptyMasterKey);

            var key = ConvertUtils.HexStringToBytes(_dataProtectionService.Decrypt(device.MasterPassword));

            await remoteDevice.Access(DateTime.UtcNow, key, accessParams);

            //todo - remove this block when FW will be fixed
            {
                await remoteDevice.Initialize();
                if (remoteDevice.AccessLevel.IsMasterKeyRequired)
                {
                    await remoteDevice.Access(DateTime.UtcNow, key, accessParams);
                    await remoteDevice.Initialize();
                }
            }

            // updating the primary account or all tasks
            if (primaryAccountOnly)
                await _remoteTaskService.ExecuteRemoteTasks(deviceId, remoteDevice, TaskOperation.Primary);
            else
                await _remoteTaskService.ExecuteRemoteTasks(deviceId, remoteDevice, TaskOperation.None);

            Debug.WriteLine($"!!!!!!!!!!!!! UpdateRemoteDevice OK");

            return true;
        }

        #endregion Device

        #region Workstation

        public async Task RegisterWorkstationInfoAsync(IRemoteAppConnection remoteAppConnection, WorkstationInfo workstationInfo)
        {
            if (workstationInfo == null)
                throw new ArgumentNullException(nameof(workstationInfo));

            _workstationConnections.AddOrUpdate(workstationInfo.Id, remoteAppConnection, (id, oldConnection) =>
            {
                return remoteAppConnection;
            });

            if (await _workstationService.ExistAsync(w => w.Id == workstationInfo.Id))
            {
                // Workstation exists, update information
                await _workstationService.UpdateWorkstationInfoAsync(workstationInfo);
            }
            else
            {
                // Workstation does not exist or name + domain was changed, create new
                await _workstationService.AddWorkstationAsync(workstationInfo);
                _logger.LogInformation($"New workstation {workstationInfo.MachineName} was added");
            }

            await _workstationService.UpdateProximitySettingsAsync(workstationInfo.Id);
            await _workstationService.UpdateRfidStateAsync(workstationInfo.Id);
        }

        public async Task OnAppHubDisconnectedAsync(string workstationId)
        {
            _workstationConnections.TryRemove(workstationId, out IRemoteAppConnection _);

            await _workstationAuditService.CloseSessionAsync(workstationId);
        }

        private static IRemoteAppConnection FindWorkstationConnection(string workstationId)
        {
            _workstationConnections.TryGetValue(workstationId, out IRemoteAppConnection workstation);
            return workstation;
        }

        public static bool IsWorkstationConnectedToServer(string workstationId)
        {
            return _workstationConnections.ContainsKey(workstationId);
        }

        public static int WorkstationsOnlineCount()
        {
            return _workstationConnections.Count;
        }

        public static async Task UpdateProximitySettingsAsync(string workstationId, IReadOnlyList<DeviceProximitySettingsDto> deviceProximitySettings)
        {
            var workstation = FindWorkstationConnection(workstationId);
            if (workstation != null)
            {
                await workstation.UpdateProximitySettings(deviceProximitySettings);
            }
        }

        public static async Task UpdateRfidIndicatorStateAsync(string workstationId, bool isEnabled)
        {
            var workstation = FindWorkstationConnection(workstationId);
            if (workstation != null)
            {
                await workstation.UpdateRFIDIndicatorState(isEnabled);
            }
        }

        #endregion
    }
}
