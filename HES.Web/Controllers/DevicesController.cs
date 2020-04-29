using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models;
using HES.Core.Models.API;
using HES.Core.Models.Web.HardwareVault;
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
        private readonly IHardwareVaultService _deviceService;
        private readonly IWorkstationAuditService _workstationAuditService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(IHardwareVaultService deviceService,
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
        public async Task<ActionResult<IEnumerable<HardwareVault>>> GetDevices()
        {
            var count = await _deviceService.GetVaultsCountAsync(string.Empty, null);
            return await _deviceService.GetVaultsAsync(0, count, nameof(HardwareVault.Id), ListSortDirection.Ascending, string.Empty, null);         
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<HardwareVault>>> GetFilteredDevices(HardwareVaultFilter hardwareVaultFilter)
        {   
            var count = await _deviceService.GetVaultsCountAsync(string.Empty, hardwareVaultFilter);
            return await _deviceService.GetVaultsAsync(0, count, nameof(HardwareVault.Id), ListSortDirection.Ascending, string.Empty, hardwareVaultFilter);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<HardwareVault>>> GetDevicesByEmployeeId(string id)
        {
            return await _deviceService.GetVaultsByEmployeeIdAsync(id);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HardwareVault>> GetDeviceById(string id)
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
                var device = new HardwareVault()
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
                await _deviceService.ChangeVaultProfileAsync(setAccessProfileDto.DeviceId, setAccessProfileDto.ProfileId);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(setAccessProfileDto.DeviceId);
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
        public async Task<ActionResult<IEnumerable<HardwareVaultProfile>>> GetAccessProfiles()
        {
            return await _deviceService.GetProfilesAsync();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HardwareVaultProfile>> GetAccessProfileById(string id)
        {
            var accessProfile = await _deviceService.GetProfileByIdAsync(id);

            if (accessProfile == null)
            {
                return NotFound();
            }

            return accessProfile;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<HardwareVaultProfile>> CreateAccessProfile(CreateDeviceAccessProfileDto deviceAccessProfileDto)
        {
            HardwareVaultProfile createdDeviceAccessProfile;
            try
            {
                var deviceAccessProfile = new HardwareVaultProfile()
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
                var deviceAccessProfile = new HardwareVaultProfile()
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
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(await _deviceService.GetVaultIdsByProfileTaskAsync(deviceAccessProfile.Id));
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
        public async Task<ActionResult<HardwareVaultProfile>> DeleteAccessProfile(string id)
        {
            var deviceAccessProfile = await _deviceService.GetProfileByIdAsync(id);
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
