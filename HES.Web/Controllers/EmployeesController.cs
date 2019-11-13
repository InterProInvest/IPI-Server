using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models;
using HES.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.AdminRole)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDeviceAccountService _deviceAccountService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(IEmployeeService employeeService,
                                   IDeviceAccountService deviceAccountService,
                                   IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                                   ILogger<EmployeesController> logger)
        {
            _employeeService = employeeService;
            _deviceAccountService = deviceAccountService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _logger = logger;
        }

        #region Employee

        [HttpGet]
        public async Task<ActionResult<IList<Employee>>> GetEmployees()
        {
            return await _employeeService.GetAllEmployeesAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployeeById(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return BadRequest(new { error = $"{ nameof(id)} is null" });
            }

            var employee = await _employeeService.GetEmployeeWithIncludeAsync(id);

            if (employee == null)
            {
                _logger.LogWarning($"{nameof(employee)} is null");
                return NotFound();
            }

            return employee;
        }

        [HttpPost]
        public async Task<ActionResult<Employee>> CreateEmployee(Employee employee)
        {
            try
            {
                await _employeeService.CreateEmployeeAsync(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return CreatedAtAction("GetEmployeeById", new { id = employee.Id }, employee);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditEmployee(string id, Employee employee)
        {
            if (id != employee.Id)
            {
                return BadRequest();
            }

            try
            {
                await _employeeService.EditEmployeeAsync(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Employee>> DeleteEmployee(string id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            try
            {
                await _employeeService.DeleteEmployeeAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return employee;
        }

        #endregion

        #region Device

        [HttpPost()]
        public async Task<IActionResult> AddDevice(string employeeId, string[] devicesIds)
        {
            try
            {
                await _employeeService.AddDeviceAsync(employeeId, devicesIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpDelete()]
        public async Task<IActionResult> RemoveDevice(string employeeId, string deviceId)
        {
            try
            {
                await _employeeService.RemoveDeviceAsync(employeeId, deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        #endregion

        #region Account

        [HttpGet("{id}")]
        public async Task<ActionResult<IList<DeviceAccount>>> GetDeviceAccounts(string id)
        {
            return await _employeeService.GetDeviceAccountsAsync(id);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DeviceAccount>> GetDeviceAccountById(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return BadRequest(new { error = $"{ nameof(id)} is null" });
            }

            var deviceAccount = await _employeeService.GetDeviceAccountByIdAsync(id);

            if (deviceAccount == null)
            {
                _logger.LogWarning($"{nameof(deviceAccount)} is null");
                return NotFound();
            }

            return deviceAccount;
        }

        [HttpPost]
        public async Task<ActionResult<Employee>> CreateDeviceAccount(DeviceAccountDto deviceAccountDto)
        {
            try
            {
                await _employeeService.CreatePersonalAccountAsync(deviceAccountDto.DeviceAccount, deviceAccountDto.AccountPassword, deviceAccountDto.DevicesIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return CreatedAtAction("GetDeviceAccountById", new { id = deviceAccountDto.DeviceAccount.Id }, deviceAccountDto.DeviceAccount);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditDeviceAccount(string id, DeviceAccount deviceAccount)
        {
            if (id != deviceAccount.Id)
            {
                return BadRequest();
            }

            try
            {
                await _employeeService.EditPersonalAccountAsync(deviceAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<DeviceAccount>> DeleteDeviceAccount(string id)
        {
            var deviceAccount = await _deviceAccountService.GetByIdAsync(id);
            if (deviceAccount == null)
            {
                return NotFound();
            }

            try
            {
                await _employeeService.DeleteAccount(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return deviceAccount;
        }

        [HttpPost]
        public async Task<IActionResult> AddSharedAccount(SharedAccountDto sharedAccountDto)
        {
            try
            {
                await _employeeService.AddSharedAccount(sharedAccountDto.EmployeeId, sharedAccountDto.SharedAccountId, sharedAccountDto.DevicesIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UndoChanges(string accountId)
        {
            try
            {
                await _employeeService.UndoChanges(accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkstationAccount(WorkstationAccountDto workstationAccountDto)
        {
            try
            {
                await _employeeService.CreateWorkstationAccountAsync(workstationAccountDto.WorkstationAccount, workstationAccountDto.EmployeeId, workstationAccountDto.DeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> SetAsWorkstationAccount(string deviceId, string deviceAccountId)
        {
            try
            {
                await _employeeService.SetPrimaryAccount(deviceId, deviceAccountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return Ok();
        }

        #endregion
    }
}