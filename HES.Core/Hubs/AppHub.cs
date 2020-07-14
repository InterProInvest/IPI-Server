using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Workstation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Hubs
{
    public class AppHub : Hub<IRemoteAppConnection>
    {
        private readonly IRemoteDeviceConnectionsService _remoteDeviceConnectionsService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly IWorkstationAuditService _workstationAuditService;
        private readonly IWorkstationService _workstationService;
        private readonly IHardwareVaultService _hardwareVaultService;
        private readonly IHardwareVaultTaskService _deviceTaskService;
        private readonly ILicenseService _licenseService;
        private readonly IEmployeeService _employeeService;
        private readonly IHubContext<RefreshHub> _hubContext;
        private readonly ILogger<AppHub> _logger;

        public AppHub(IRemoteDeviceConnectionsService remoteDeviceConnectionsService,
                      IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                      IWorkstationAuditService workstationAuditService,
                      IWorkstationService workstationService,
                      IHardwareVaultService hardwareVaultService,
                      IHardwareVaultTaskService deviceTaskService,
                      ILicenseService licenseService,
                      IEmployeeService employeeService,
                      IHubContext<RefreshHub> hubContext,
                      ILogger<AppHub> logger)
        {
            _remoteDeviceConnectionsService = remoteDeviceConnectionsService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _workstationAuditService = workstationAuditService;
            _workstationService = workstationService;
            _hardwareVaultService = hardwareVaultService;
            _deviceTaskService = deviceTaskService;
            _licenseService = licenseService;
            _employeeService = employeeService;
            _hubContext = hubContext;
            _logger = logger;
        }

        #region Workstation

        public override Task OnConnectedAsync()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                string workstationId = httpContext.Request.Headers["WorkstationId"].ToString();

                if (string.IsNullOrWhiteSpace(workstationId))
                    throw new Exception($"AppHub.OnConnectedAsync - httpContext.Request.Headers does not contain WorkstationId");

                _remoteDeviceConnectionsService.OnAppHubConnected(workstationId, Clients.Caller);
                Context.Items.Add("WorkstationId", workstationId);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
            }

            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var workstationId = GetWorkstationId();

                _remoteDeviceConnectionsService.OnAppHubDisconnected(workstationId);
                await _remoteWorkstationConnectionsService.OnAppHubDisconnectedAsync(workstationId);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Incomming request
        public async Task<HesResponse> RegisterWorkstationInfo(WorkstationInfo workstationInfo)
        {
            try
            {
                await _remoteWorkstationConnectionsService.RegisterWorkstationInfoAsync(Clients.Caller, workstationInfo);

                // Workstation not approved
                if (!await _workstationService.CheckIsApprovedAsync(workstationInfo.Id))
                    return new HesResponse(HideezErrorCode.HesWorkstationNotApproved, $"Workstation not approved");

                return HesResponse.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{workstationInfo?.MachineName}] {ex.Message}");
                return new HesResponse(ex);
            }
        }

        // Incomming request
        public async Task<HesResponse> SaveClientEvents(WorkstationEventDto[] workstationEventsDto)
        {
            // Workstation not approved
            if (!await _workstationService.CheckIsApprovedAsync(GetWorkstationId()))
                return new HesResponse(HideezErrorCode.HesWorkstationNotApproved, $"Workstation not approved");

            if (workstationEventsDto == null)
                return new HesResponse(new ArgumentNullException(nameof(workstationEventsDto)));

            foreach (var eventDto in workstationEventsDto)
            {
                try
                {
                    await _workstationAuditService.AddEventDtoAsync(eventDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[Add Event] {ex.Message}");
                }

                try
                {
                    await _workstationAuditService.AddOrUpdateWorkstationSession(eventDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[Add/Update Session] {ex.Message}");
                }
            }

            return HesResponse.Ok;
        }

        private string GetWorkstationId()
        {
            if (Context.Items.TryGetValue("WorkstationId", out object workstationId))
                return (string)workstationId;
            else
                throw new Exception("AppHub does not contain WorkstationId!");
        }

        #endregion

        #region Device

        // Incoming request
        public async Task<HesResponse> OnDeviceConnected(BleDeviceDto dto)
        {
            try
            {
                // Workstation not approved
                if (!await _workstationService.CheckIsApprovedAsync(GetWorkstationId()))
                    return new HesResponse(HideezErrorCode.HesWorkstationNotApproved, $"Workstation not approved.");

                if (dto?.DeviceSerialNo == null)
                    throw new ArgumentNullException(nameof(dto.DeviceSerialNo));

                _remoteDeviceConnectionsService.OnDeviceConnected(dto.DeviceSerialNo, GetWorkstationId(), Clients.Caller);

                await OnDevicePropertiesChanged(dto);

                if (dto.LicenseInfo == 0)
                    return HesResponse.Ok;

                await InvokeVaultStateChanged(dto.DeviceSerialNo);
                await CheckVaultStatusAsync(dto);
                await _remoteDeviceConnectionsService.SyncHardwareVaults(dto.DeviceSerialNo);
                return HesResponse.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{dto.DeviceSerialNo}] {ex.Message}");
                return new HesResponse(ex);
            }
        }

        // Incoming request
        public async Task<HesResponse> OnDeviceDisconnected(string vaultId)
        {
            try
            {
                if (!string.IsNullOrEmpty(vaultId))
                {
                    await InvokeVaultStateChanged(vaultId);
                    _remoteDeviceConnectionsService.OnDeviceDisconnected(vaultId, GetWorkstationId());
                    await _employeeService.UpdateLastSeenAsync(vaultId);
                }
                return HesResponse.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{vaultId}] {ex.Message}");
                return new HesResponse(ex);
            }
        }

        // Incomming request
        public async Task<HesResponse> OnDevicePropertiesChanged(BleDeviceDto dto)
        {
            try
            {
                if (dto.DeviceSerialNo == null)
                    throw new ArgumentNullException(nameof(dto.DeviceSerialNo));

                await _hardwareVaultService.UpdateHardwareVaultInfoAsync(dto);

                return HesResponse.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{dto.DeviceSerialNo}] {ex.Message}");
                return new HesResponse(ex);
            }
        }

        private async Task CheckVaultStatusAsync(BleDeviceDto dto)
        {
            var vault = await _hardwareVaultService.GetVaultByIdAsync(dto.DeviceSerialNo);

            if (vault == null)
                return;

            if (vault.Status == VaultStatus.Deactivated || vault.Status == VaultStatus.Compromised ||
                vault.Status == VaultStatus.Suspended || vault.Status == VaultStatus.Reserved)
                await _remoteWorkstationConnectionsService.UpdateRemoteDeviceAsync(dto.DeviceSerialNo, GetWorkstationId(), primaryAccountOnly: false);
        }

        private async Task InvokeVaultStateChanged(string vaultId)
        {
            await _hubContext.Clients.All.SendAsync(RefreshPage.HardwareVaultStateChanged);

            var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);

            if (vault != null && vault?.EmployeeId != null)
                await _hubContext.Clients.All.SendAsync(RefreshPage.EmployeesDetailsVaultState, vault.EmployeeId);
        }

        // Incomming request
        public async Task<HesResponse<DeviceInfoDto>> GetInfoByRfid(string rfid)
        {
            try
            {
                var device = await _hardwareVaultService
                    .VaultQuery()
                    .Include(d => d.Employee)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.RFID == rfid);

                var info = await GetDeviceInfo(device);
                return new HesResponse<DeviceInfoDto>(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new HesResponse<DeviceInfoDto>(ex);
            }
        }

        // Incomming request
        public async Task<HesResponse<DeviceInfoDto>> GetInfoByMac(string mac)
        {
            try
            {
                var device = await _hardwareVaultService
                    .VaultQuery()
                    .Include(d => d.Employee)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.MAC == mac);

                var info = await GetDeviceInfo(device);
                return new HesResponse<DeviceInfoDto>(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new HesResponse<DeviceInfoDto>(ex);
            }
        }

        // Incomming request
        public async Task<HesResponse<DeviceInfoDto>> GetInfoBySerialNo(string deviceId)
        {
            try
            {
                var device = await _hardwareVaultService
                    .VaultQuery()
                    .Include(d => d.Employee)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == deviceId);

                var info = await GetDeviceInfo(device);
                return new HesResponse<DeviceInfoDto>(info);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{deviceId}] {ex.Message}");
                return new HesResponse<DeviceInfoDto>(ex);
            }
        }

        private async Task<DeviceInfoDto> GetDeviceInfo(HardwareVault device)
        {
            if (device == null)
                return null;

            bool needUpdate = await _deviceTaskService
                .TaskQuery()
                .Where(t => t.HardwareVaultId == device.Id)
                .AsNoTracking()
                .AnyAsync();

            var info = new DeviceInfoDto()
            {
                OwnerName = device.Employee?.FullName,
                OwnerEmail = device.Employee?.Email,
                DeviceMac = device.MAC,
                DeviceSerialNo = device.Id,
                NeedUpdate = needUpdate,
                HasNewLicense = device.HasNewLicense,
                DeviceCompromised = device.Status == VaultStatus.Compromised ? true : false
            };

            return info;
        }

        // Incoming request
        public async Task<HesResponse> FixDevice(string deviceId)
        {
            try
            {
                await _remoteWorkstationConnectionsService.UpdateRemoteDeviceAsync(deviceId, GetWorkstationId(), primaryAccountOnly: true);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(deviceId);
                return HesResponse.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{deviceId}] {ex.Message}");
                return new HesResponse(ex);
            }
        }

        // Incoming request
        public async Task<HesResponse<IList<DeviceLicenseDTO>>> GetNewDeviceLicenses(string deviceId)
        {
            try
            {
                var licenses = await _licenseService.GetNotAppliedLicensesByHardwareVaultIdAsync(deviceId);

                var deviceLicenseDto = new List<DeviceLicenseDTO>();

                foreach (var license in licenses)
                {
                    deviceLicenseDto.Add(new DeviceLicenseDTO
                    {
                        Id = license.Id,
                        DeviceId = license.HardwareVaultId,
                        ImportedAt = license.ImportedAt,
                        AppliedAt = license.AppliedAt,
                        EndDate = license.EndDate,
                        LicenseOrderId = license.LicenseOrderId,
                        Data = license.Data,
                    });
                }

                return new HesResponse<IList<DeviceLicenseDTO>>(deviceLicenseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{deviceId}] {ex.Message}");
                return new HesResponse<IList<DeviceLicenseDTO>>(ex);
            }
        }

        // Incoming request
        public async Task<HesResponse> OnDeviceLicenseApplied(string deviceId, string licenseId)
        {
            try
            {
                await _licenseService.ChangeLicenseAppliedAsync(deviceId, licenseId);
                return HesResponse.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{deviceId}] {ex.Message}");
                return new HesResponse(ex);
            }
        }

        // Incoming request
        public async Task<HesResponse<IList<DeviceLicenseDTO>>> GetDeviceLicenses(string deviceId)
        {
            try
            {
                var licenses = await _licenseService.GetActiveLicensesAsync(deviceId);

                var deviceLicenseDto = new List<DeviceLicenseDTO>();

                foreach (var license in licenses)
                {
                    deviceLicenseDto.Add(new DeviceLicenseDTO
                    {
                        Id = license.Id,
                        DeviceId = license.HardwareVaultId,
                        ImportedAt = license.ImportedAt,
                        AppliedAt = license.AppliedAt,
                        EndDate = license.EndDate,
                        LicenseOrderId = license.LicenseOrderId,
                        Data = license.Data,
                    });
                }

                return new HesResponse<IList<DeviceLicenseDTO>>(deviceLicenseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{deviceId}] {ex.Message}");
                return new HesResponse<IList<DeviceLicenseDTO>>(ex);
            }
        }

        #endregion
    }
}