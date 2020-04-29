using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Utils;
using Hideez.SDK.Communication.Workstation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
        readonly IWorkstationService _workstationService;
        readonly IHardwareVaultService _deviceService;
        readonly IDeviceTaskService _deviceTaskService;
        readonly IDataProtectionService _dataProtectionService;
        readonly IWorkstationAuditService _workstationAuditService;
        readonly ILogger<RemoteWorkstationConnectionsService> _logger;

        public RemoteWorkstationConnectionsService(IServiceProvider services,
                      IRemoteTaskService remoteTaskService,
                      IRemoteDeviceConnectionsService remoteDeviceConnectionsService,
                      IWorkstationService workstationService,
                      IHardwareVaultService deviceService,
                      IDeviceTaskService deviceTaskService,
                      IDataProtectionService dataProtectionService,
                      IWorkstationAuditService workstationAuditService,
                      ILogger<RemoteWorkstationConnectionsService> logger)
        {
            _services = services;
            _remoteTaskService = remoteTaskService;
            _remoteDeviceConnectionsService = remoteDeviceConnectionsService;
            _workstationService = workstationService;
            _deviceService = deviceService;
            _deviceTaskService = deviceTaskService;
            _dataProtectionService = dataProtectionService;
            _workstationAuditService = workstationAuditService;
            _logger = logger;
        }

        #region Hardware Vault

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
                    var remoteWorkstationConnectionsService = scope.ServiceProvider.GetRequiredService<IRemoteWorkstationConnectionsService>();
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
                await tcs.Task;
                return;
            }

            try
            {
                await UpdateRemoteDevice(deviceId, workstationId, primaryAccountOnly);
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
                throw;
            }
            finally
            {
                _devicesInProgress.TryRemove(deviceId, out TaskCompletionSource<bool> _);
            }
        }

        private async Task<bool> UpdateRemoteDevice(string vaultId, string workstationId, bool primaryAccountOnly)
        {
            // TODO ignore not approved workstations
            //throw new HideezException(HideezErrorCode.HesWorkstationNotApproved);

            var remoteDevice = await _remoteDeviceConnectionsService
               .ConnectDevice(vaultId, workstationId)
               .TimeoutAfter(30_000);

            if (remoteDevice == null)
                throw new HideezException(HideezErrorCode.HesFailedEstablishRemoteDeviceConnection);

            var vault = await _deviceService.VaultQuery().AsNoTracking().FirstOrDefaultAsync(d => d.Id == vaultId);

            if (vault == null)
                throw new HideezException(HideezErrorCode.HesDeviceNotFound);

            switch (vault.Status)
            {
                case VaultStatus.Ready:
                    throw new HideezException(HideezErrorCode.HesDeviceNotAssignedToAnyUser);
                case VaultStatus.Reserved:
                    await _remoteTaskService.ExecuteRemoteTasks(vault.Id, remoteDevice, TaskOperation.Link);
                    await CheckPassphraseAsync(remoteDevice, vault.Id);
                    await CheckTaskAsync(remoteDevice, vault.Id, primaryAccountOnly);
                    break;
                case VaultStatus.Active:
                    await CheckPassphraseAsync(remoteDevice, vault.Id);
                    await CheckTaskAsync(remoteDevice, vault.Id, primaryAccountOnly);
                    break;
                case VaultStatus.Locked:
                    await _remoteTaskService.ExecuteRemoteTasks(vault.Id, remoteDevice, TaskOperation.Suspend);
                    throw new Exception($"Vault {vault.Id} is locked.");
                case VaultStatus.Suspended:
                    throw new Exception($"Vault {vault.Id} is locked.");
                case VaultStatus.Deactivated:
                    await remoteDevice.Wipe(ConvertUtils.HexStringToBytes(vault.MasterPassword));
                    await _deviceService.UpdateAfterWipe(vault.Id);
                    throw new HideezException(HideezErrorCode.DeviceHasBeenWiped);
                case VaultStatus.Compromised:
                    throw new Exception($"Vault {vault.Id} in Compromised status.");
                default:
                    _logger.LogCritical($"Unhandled vault status ({vault.Status})");
                    throw new Exception("Unhandled vault status.");
            }

            //await CheckLockedAsync(remoteDevice, vault);
            //await CheckLinkedAsync(remoteDevice, vault);
            //await CheckPassphraseAsync(remoteDevice, vaultId);
            //await CheckTaskAsync(remoteDevice, deviceId, primaryAccountOnly);

            return true;
        }

        private async Task CheckLockedAsync(RemoteDevice remoteDevice, HardwareVault vault)
        {
            if (remoteDevice.AccessLevel.IsLocked)
            {
                await _remoteTaskService.ExecuteRemoteTasks(vault.Id, remoteDevice, TaskOperation.Suspend);
                throw new Exception($"Vault {vault.Id} is locked");
            }
        }

        private async Task CheckLinkedAsync(RemoteDevice remoteDevice, HardwareVault vault)
        {
            if (!remoteDevice.AccessLevel.IsLinkRequired)
            {
                if (vault.MasterPassword != null)
                {
                    return;
                }
                else
                {
                    throw new Exception("This vault is linked to another server");
                }
            }
            else
            {
                if (vault.MasterPassword != null)
                {
                    throw new Exception("The vault was wiped a non-current server");
                }
                else
                {
                    var existLinkTask = await _deviceTaskService
                        .TaskQuery()
                        .Where(d => d.HardwareVaultId == vault.Id && d.Operation == TaskOperation.Link)
                        .AsNoTracking()
                        .AnyAsync();

                    if (existLinkTask)
                    {
                        await _remoteTaskService.ExecuteRemoteTasks(vault.Id, remoteDevice, TaskOperation.Link);
                        await remoteDevice.RefreshDeviceInfo();

                        if (remoteDevice.AccessLevel.IsLinkRequired)
                        {
                            throw new Exception($"Can't link the vault {vault.Id}, after executing the link task.");
                        }
                    }
                    else
                    {
                        throw new HideezException(HideezErrorCode.HesDeviceNotAssignedToAnyUser);
                    }
                }
            }
        }

        private async Task CheckPassphraseAsync(RemoteDevice remoteDevice, string vaultId)
        {
            var vault = await _deviceService.VaultQuery().AsNoTracking().FirstOrDefaultAsync(d => d.Id == vaultId);

            if (vault == null)
                throw new Exception($"Vault {vaultId} not found.");

            if (!remoteDevice.AccessLevel.IsLinkRequired && vault.MasterPassword == null)
                throw new Exception($"Vault {vaultId} is linked to another server");

            if (remoteDevice.AccessLevel.IsLinkRequired && vault.MasterPassword != null)
                throw new Exception($"Vault {vaultId} was wiped a non-current server");

            var key = ConvertUtils.HexStringToBytes(_dataProtectionService.Decrypt(vault.MasterPassword));

            await remoteDevice.CheckPassphrase(key);
        }

        private async Task CheckTaskAsync(RemoteDevice remoteDevice, string deviceId, bool primaryAccountOnly)
        {
            if (primaryAccountOnly)
            {
                await _remoteTaskService.ExecuteRemoteTasks(deviceId, remoteDevice, TaskOperation.Primary);
            }
            else
            {
                await _remoteTaskService.ExecuteRemoteTasks(deviceId, remoteDevice, TaskOperation.None);
            }
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