using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models;
using HES.Core.Models.API;
using HES.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.AdminRole)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        private readonly IWorkstationAuditService _workstationAuditService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(IDeviceService deviceService,
                                 IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                                 IWorkstationAuditService workstationAuditService,
                                 ILogger<DevicesController> logger)
        {
            _deviceService = deviceService;
            _workstationAuditService = workstationAuditService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _logger = logger;
        }

        #region Device

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Device>>> GetDevices()
        {
            var count = await _deviceService.GetVaultsCountAsync(string.Empty, null);
            return await _deviceService.GetVaultsAsync(0, count, nameof(Device.Id), ListSortDirection.Ascending, string.Empty, null);         
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Device>>> GetFilteredDevices(DeviceFilter deviceFilter)
        {
            return await _deviceService.GetFilteredDevicesAsync(deviceFilter);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Device>>> GetDevicesByEmployeeId(string id)
        {
            return await _deviceService.GetVaultsByEmployeeIdAsync(id);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Device>> GetDeviceById(string id)
        {
            var device = await _deviceService.GetVaultByIdAsync(id);

            if (device == null)
            {
                return NotFound();
            }

            return device;
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditDevice(string id, EditDeviceDto deviceDto)
        {
            if (id != deviceDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var device = new Device()
                {
                    Id = deviceDto.Id,
                    RFID = deviceDto.RFID
                };

                await _deviceService.EditRfidAsync(device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
              return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> SetAccessProfile(SetDeviceAccessProfileDto setAccessProfileDto)
        {
            try
            {
                var devices = new string[] { setAccessProfileDto.DeviceId };
                await _deviceService.SetProfileAsync(devices, setAccessProfileDto.ProfileId);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
              return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpPost("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UnlockPin(string id)
        {
            try
            {
                // TODOSTATUS
                //await _deviceService.UnlockPinAsync(id);
                await _workstationAuditService.AddPendingUnlockEventAsync(id);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
              return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        #endregion

        #region Access Profile

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DeviceAccessProfile>>> GetAccessProfiles()
        {
            return await _deviceService.GetAccessProfilesAsync();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeviceAccessProfile>> GetAccessProfileById(string id)
        {
            var accessProfile = await _deviceService.GetAccessProfileByIdAsync(id);

            if (accessProfile == null)
            {
                return NotFound();
            }

            return accessProfile;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<DeviceAccessProfile>> CreateAccessProfile(CreateDeviceAccessProfileDto deviceAccessProfileDto)
        {
            DeviceAccessProfile createdDeviceAccessProfile;
            try
            {
                var deviceAccessProfile = new DeviceAccessProfile()
                {
                    Name = deviceAccessProfileDto.Name,
                    ButtonBonding = deviceAccessProfileDto.ButtonBonding,
                    ButtonConnection = deviceAccessProfileDto.ButtonConnection,
                    ButtonNewChannel = deviceAccessProfileDto.ButtonNewChannel,
                    PinBonding = deviceAccessProfileDto.PinBonding,
                    PinConnection = deviceAccessProfileDto.PinConnection,
                    PinNewChannel = deviceAccessProfileDto.PinNewChannel,
                    MasterKeyConnection = deviceAccessProfileDto.MasterKeyConnection,
                    MasterKeyNewChannel = deviceAccessProfileDto.MasterKeyNewChannel,
                    PinExpiration = deviceAccessProfileDto.PinExpiration,
                    PinLength = deviceAccessProfileDto.PinLength,
                    PinTryCount = deviceAccessProfileDto.PinTryCount
                };
                createdDeviceAccessProfile = await _deviceService.CreateProfileAsync(deviceAccessProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
              return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetAccessProfileById", new { id = createdDeviceAccessProfile.Id }, createdDeviceAccessProfile);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditAccessProfile(string id, EditDeviceAccessProfileDto deviceAccessProfileDto)
        {
            if (id != deviceAccessProfileDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var deviceAccessProfile = new DeviceAccessProfile()
                {
                    Id = deviceAccessProfileDto.Id,
                    Name = deviceAccessProfileDto.Name,
                    ButtonBonding = deviceAccessProfileDto.ButtonBonding,
                    ButtonConnection = deviceAccessProfileDto.ButtonConnection,
                    ButtonNewChannel = deviceAccessProfileDto.ButtonNewChannel,
                    PinBonding = deviceAccessProfileDto.PinBonding,
                    PinConnection = deviceAccessProfileDto.PinConnection,
                    PinNewChannel = deviceAccessProfileDto.PinNewChannel,
                    MasterKeyConnection = deviceAccessProfileDto.MasterKeyConnection,
                    MasterKeyNewChannel = deviceAccessProfileDto.MasterKeyNewChannel,
                    PinExpiration = deviceAccessProfileDto.PinExpiration,
                    PinLength = deviceAccessProfileDto.PinLength,
                    PinTryCount = deviceAccessProfileDto.PinTryCount
                };
                await _deviceService.EditProfileAsync(deviceAccessProfile);
                var devicesIds = await _deviceService.UpdateProfileAsync(deviceAccessProfile.Id);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devicesIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
              return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeviceAccessProfile>> DeleteAccessProfile(string id)
        {
            var deviceAccessProfile = await _deviceService.GetAccessProfileByIdAsync(id);
            if (deviceAccessProfile == null)
            {
                return NotFound();
            }

            try
            {
                await _deviceService.DeleteProfileAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
              return StatusCode(500, new { error = ex.Message });
            }

            return deviceAccessProfile;
        }

        #endregion
    }
}
