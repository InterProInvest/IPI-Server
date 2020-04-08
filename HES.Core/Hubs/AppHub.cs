using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Workstation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HES.Core.Hubs
{
    public class AppHub : Hub<IRemoteAppConnection>
    {
        private readonly IRemoteDeviceConnectionsService _remoteDeviceConnectionsService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly IWorkstationAuditService _workstationAuditService;
        private readonly IDeviceService _deviceService;
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly ILicenseService _licenseService;
        private readonly IEmployeeService _employeeService;
        private readonly IAsyncProxyRequestService _proxyRequestService;
        private readonly ILogger<AppHub> _logger;

        public AppHub(IRemoteDeviceConnectionsService remoteDeviceConnectionsService,
                      IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                      IWorkstationAuditService workstationAuditService,
                      IDeviceService deviceService,
                      IDeviceTaskService deviceTaskService,
                      ILicenseService licenseService,
                      IEmployeeService employeeService,
                      IAsyncProxyRequestService proxyRequestService,
                      ILogger<AppHub> logger)
        {
            _remoteDeviceConnectionsService = remoteDeviceConnectionsService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _workstationAuditService = workstationAuditService;
            _deviceService = deviceService;
            _deviceTaskService = deviceTaskService;
            _licenseService = licenseService;
            _employeeService = employeeService;
            _proxyRequestService = proxyRequestService;
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

        private string GetWorkstationId()
        {
            if (Context.Items.TryGetValue("WorkstationId", out object workstationId))
                return (string)workstationId;
            else
                throw new Exception("AppHub does not contain WorkstationId!");
        }

        // Incomming request
        public async Task<HesResponse> RegisterWorkstationInfo(WorkstationInfo workstationInfo)
        {
            try
            {
                await _remoteWorkstationConnectionsService.RegisterWorkstationInfoAsync(Clients.Caller, workstationInfo);
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
            if (workstationEventsDto == null)
                return new HesResponse(new ArgumentNullException(nameof(workstationEventsDto)));

            // Ignore not approved workstation
            //var workstationId = workstationEventsDto.FirstOrDefault().WorkstationId;
            //var workstaton = await _workstationService.GetByIdAsync(workstationId);
            //if (workstaton.Approved == false)
            //{
            //    return new HideezErrorInfo(HideezErrorCode.HesWorkstationNotApproved, $"{workstationId}");
            //}

            foreach (var eventDto in workstationEventsDto)
            {
                try
                {
                    await _workstationAuditService.AddEventDtoAsync(eventDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[AddEventDtoAsync] {ex.Message}");
                }

                try
                {
                    await _workstationAuditService.AddOrUpdateWorkstationSession(eventDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[AddOrUpdateWorkstationSession] {ex.Message}");
                }
            }

            return HesResponse.Ok;
        }

        #endregion

        #region Device

        // Incoming request
        public async Task<HesResponse> OnDeviceConnected(BleDeviceDto dto)
        {
            try
            {
                await OnDevicePropertiesChanged(dto);
                _remoteDeviceConnectionsService.OnDeviceConnected(dto.DeviceSerialNo, GetWorkstationId(), Clients.Caller);
                return HesResponse.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{dto.DeviceSerialNo}] {ex.Message}");
                return new HesResponse(ex);
            }
        }

        // Incoming request
        public async Task<HesResponse> OnDeviceDisconnected(string deviceId)
        {
            try
            {
                if (!string.IsNullOrEmpty(deviceId))
                {
                    _remoteDeviceConnectionsService.OnDeviceDisconnected(deviceId, GetWorkstationId());
                    await _employeeService.UpdateLastSeenAsync(deviceId);
                }
                return HesResponse.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{deviceId}] {ex.Message}");
                return new HesResponse(ex);
            }
        }

        // Incomming request
        public async Task<HesResponse> OnDevicePropertiesChanged(BleDeviceDto dto)
        {
            try
            {
                if (dto == null || dto.DeviceSerialNo == null)
                    throw new ArgumentNullException(nameof(dto));

                var exist = await _deviceService
                    .DeviceQuery()
                    .AsNoTracking()
                    .Where(d => d.Id == dto.DeviceSerialNo)
                    .AnyAsync();

                if (exist)
                {
                    // Update Battery, Firmware, IsLocked, LastSynced         
                    await _deviceService.UpdateDeviceInfoAsync(dto.DeviceSerialNo, dto.Battery, dto.FirmwareVersion, dto.IsLocked);
                }
                else
                {
                    if (dto.Mac != null)
                    {
                        // Add device
                        var device = new Device()
                        {
                            Id = dto.DeviceSerialNo,
                            MAC = dto.Mac,
                            Model = dto.DeviceSerialNo.Substring(0, 5),
                            RFID = dto.Mac.Replace(":", "").Substring(0, 10),
                            Battery = dto.Battery,
                            Firmware = dto.FirmwareVersion,
                            ImportedAt = DateTime.UtcNow,
                            LastSynced = DateTime.UtcNow
                        };
                        await _deviceService.AddDeviceAsync(device);
                    }
                }
                return HesResponse.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{dto.DeviceSerialNo}] {ex.Message}");
                return new HesResponse(ex);
            }
        }

        // Incomming request
        public async Task<HesResponse<DeviceInfoDto>> GetInfoByRfid(string rfid)
        {
            try
            {
                var device = await _deviceService
                    .DeviceQuery()
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
                var device = await _deviceService
                    .DeviceQuery()
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
                var device = await _deviceService
                    .DeviceQuery()
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

        private async Task<DeviceInfoDto> GetDeviceInfo(Device device)
        {
            if (device == null)
                return null;

            bool needUpdate = await _deviceTaskService
                .Query()
                .Where(t => t.DeviceId == device.Id)
                .AsNoTracking()
                .AnyAsync();

            var info = new DeviceInfoDto()
            {
                OwnerName = device.Employee?.FullName,
                OwnerEmail = device.Employee?.Email,
                DeviceMac = device.MAC,
                DeviceSerialNo = device.Id,
                NeedUpdate = needUpdate,
                HasNewLicense = device.HasNewLicense
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
                var licenses = await _licenseService.GetDeviceLicensesByDeviceIdAsync(deviceId);

                var deviceLicenseDto = new List<DeviceLicenseDTO>();

                foreach (var license in licenses)
                {
                    deviceLicenseDto.Add(new DeviceLicenseDTO
                    {
                        Id = license.Id,
                        DeviceId = license.DeviceId,
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
                await _licenseService.SetDeviceLicenseAppliedAsync(deviceId, licenseId);
                return HesResponse.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{deviceId}] {ex.Message}");
                return new HesResponse(ex);
            }
        }

        // Incomming request
        public Task<HesResponse> OnUnlockWorkstationResponse(string requestId, HideezErrorCode code, string message)
        {
            try
            {
                _proxyRequestService.SetRequestResponse(requestId, new HesResponse(code, message));
                return Task.FromResult(HesResponse.Ok);
            }
            catch (Exception ex)
            {
                return Task.FromResult(new HesResponse(ex));
            }
        }
        #endregion
    }
}