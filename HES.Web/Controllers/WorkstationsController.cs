using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.API;
using HES.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.AdminRole)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WorkstationsController : ControllerBase
    {
        private readonly IWorkstationService _workstationService;
        private readonly ILogger<WorkstationsController> _logger;

        public WorkstationsController(IWorkstationService workstationService, ILogger<WorkstationsController> logger)
        {
            _workstationService = workstationService;
            _logger = logger;
        }

        #region Workstation

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Workstation>>> GetWorkstations()
        {
            return await _workstationService.GetWorkstationsAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Workstation>> GetWorkstationById(string id)
        {
            var workstation = await _workstationService.GetWorkstationByIdAsync(id);

            if (workstation == null)
            {
                return NotFound();
            }

            return workstation;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditWorkstation(string id, UpdateWorkstationDto workstationDto)
        {
            if (id != workstationDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var workstation = new Workstation()
                {
                    Id = workstationDto.Id,
                    DepartmentId = workstationDto.DepartmentId,
                    RFID = workstationDto.RfidEnabled
                };
                await _workstationService.EditWorkstationAsync(workstation);
                await _workstationService.UpdateRfidStateAsync(workstation.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ApproveWorkstation(string id, UpdateWorkstationDto workstationDto)
        {
            if (id != workstationDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var workstation = new Workstation()
                {
                    Id = workstationDto.Id,
                    DepartmentId = workstationDto.DepartmentId,
                    RFID = workstationDto.RfidEnabled,
                    Approved = true
                };
                await _workstationService.ApproveWorkstationAsync(workstation);
                await _workstationService.UpdateRfidStateAsync(workstation.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UnapproveWorkstation(string id)
        {
            try
            {
                await _workstationService.UnapproveWorkstationAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        #endregion

        #region Proximity Device

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<ProximityDevice>>> GetProximityDevices(string id)
        {
            return await _workstationService.GetProximityDevicesAsync(id);
        }

        [HttpPost]
        public async Task<IActionResult> AddProximityDevice(AddProximityDeviceDto proximityDeviceDto)
        {
            try
            {
                await _workstationService.AddProximityDevicesAsync(proximityDeviceDto.WorkstationId, new string[] { proximityDeviceDto.DeviceId });
                await _workstationService.UpdateProximitySettingsAsync(proximityDeviceDto.WorkstationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteProximityDevice(string id)
        {
            try
            {
                await _workstationService.DeleteProximityDeviceAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        #endregion
    }
}