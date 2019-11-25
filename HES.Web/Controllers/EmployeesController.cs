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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            return await _employeeService.GetAllEmployeesAsync();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Employee>> GetEmployeeById(string id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
            {
                _logger.LogWarning($"{nameof(employee)} is null");
                return NotFound();
            }

            return employee;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Employee>>> GetFilteredEmployees(EmployeeFilter employeeFilter)
        {
            return await _employeeService.GetFilteredEmployeesAsync(employeeFilter);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<Employee>> CreateEmployee(CreateEmployeeDto employeeDto)
        {
            Employee createdEmployee;
            try
            {
                var employee = new Employee()
                {
                    FirstName = employeeDto.FirstName,
                    LastName = employeeDto.LastName,
                    Email = employeeDto.Email,
                    PhoneNumber = employeeDto.PhoneNumber,
                    DepartmentId = employeeDto.DepartmentId,
                    PositionId = employeeDto.PositionId
                };
                createdEmployee = await _employeeService.CreateEmployeeAsync(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetEmployeeById", new { id = createdEmployee.Id }, createdEmployee);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditEmployee(string id, EditEmployeeDto employeeDto)
        {
            if (id != employeeDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var employee = new Employee()
                {
                    Id = employeeDto.Id,
                    FirstName = employeeDto.FirstName,
                    LastName = employeeDto.LastName,
                    Email = employeeDto.Email,
                    PhoneNumber = employeeDto.PhoneNumber,
                    DepartmentId = employeeDto.DepartmentId,
                    PositionId = employeeDto.PositionId
                };
                await _employeeService.EditEmployeeAsync(employee);
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
                return StatusCode(500, new { error = ex.Message });
            }

            return employee;
        }

        #endregion

        #region Device

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AddDevice(DeviceDto deviceDto)
        {
            try
            {
                await _employeeService.AddDeviceAsync(deviceDto.EmployeeId, new string[] { deviceDto.DeviceId });
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(deviceDto.DeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpDelete()]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveDevice(DeviceDto deviceDto)
        {
            try
            {
                await _employeeService.RemoveDeviceAsync(deviceDto.EmployeeId, deviceDto.DeviceId);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(deviceDto.DeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        #endregion

        #region Account

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DeviceAccount>>> GetDeviceAccountsByEmployeeId(string id)
        {
            return await _employeeService.GetDeviceAccountsByEmployeeIdAsync(id);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeviceAccount>> GetDeviceAccountById(string id)
        {
            var deviceAccount = await _employeeService.GetDeviceAccountByIdAsync(id);

            if (deviceAccount == null)
            {
                _logger.LogWarning($"{nameof(deviceAccount)} is null");
                return NotFound();
            }

            return deviceAccount;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<DeviceAccount>> CreateDeviceAccount(CreateDeviceAccountDto deviceAccountDto)
        {
            IList<DeviceAccount> createdDeviceAccounts;
            try
            {
                var deviceAccount = new DeviceAccount()
                {
                    Name = deviceAccountDto.Name,
                    Urls = deviceAccountDto.Urls,
                    Apps = deviceAccountDto.Apps,
                    Login = deviceAccountDto.Login,
                    Kind = AccountKind.WebApp,
                    EmployeeId = deviceAccountDto.EmployeeId,
                    DeviceId = deviceAccountDto.DeviceId
                };
                var accountPassword = new AccountPassword()
                {
                    Password = deviceAccountDto.Password,
                    OtpSecret = deviceAccountDto.OtpSecret
                };

                createdDeviceAccounts = await _employeeService.CreatePersonalAccountAsync(deviceAccount, accountPassword, new string[] { deviceAccountDto.DeviceId });
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(deviceAccountDto.DeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetDeviceAccountById", new { id = createdDeviceAccounts[0].Id }, createdDeviceAccounts[0]);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditDeviceAccount(string id, EditDeviceAccountDto deviceAccountDto)
        {
            if (id != deviceAccountDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var deviceAccount = new DeviceAccount()
                {
                    Id = deviceAccountDto.Id,
                    Name = deviceAccountDto.Name,
                    Urls = deviceAccountDto.Urls,
                    Apps = deviceAccountDto.Apps,
                    Login = deviceAccountDto.Login,
                    Kind = AccountKind.WebApp,
                    DeviceId = deviceAccountDto.DeviceId
                };

                await _employeeService.EditPersonalAccountAsync(deviceAccount);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(deviceAccountDto.DeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditDeviceAccountPassword(string id, EditDeviceAccountPasswordDto deviceAccountDto)
        {
            if (id != deviceAccountDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var deviceAccount = new DeviceAccount()
                {
                    Id = deviceAccountDto.Id,
                    Kind = AccountKind.WebApp,
                    DeviceId = deviceAccountDto.DeviceId
                };
                var accountPassword = new AccountPassword()
                {
                    Password = deviceAccountDto.Password
                };

                await _employeeService.EditPersonalAccountPwdAsync(deviceAccount, accountPassword);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(deviceAccountDto.DeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditDeviceAccountOtp(string id, EditDeviceAccountOtpDto deviceAccountDto)
        {
            if (id != deviceAccountDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var deviceAccount = new DeviceAccount()
                {
                    Id = deviceAccountDto.Id,
                    Kind = AccountKind.WebApp,
                    DeviceId = deviceAccountDto.DeviceId
                };
                var accountPassword = new AccountPassword()
                {
                    OtpSecret = deviceAccountDto.OtpSercret
                };

                await _employeeService.EditPersonalAccountPwdAsync(deviceAccount, accountPassword);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(deviceAccountDto.DeviceId);
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
        public async Task<ActionResult<DeviceAccount>> DeleteDeviceAccount(string id)
        {
            var deviceAccount = await _deviceAccountService.GetByIdAsync(id);
            if (deviceAccount == null)
            {
                return NotFound();
            }

            try
            {
                var deviceId = await _employeeService.DeleteAccountAsync(id);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return deviceAccount;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AddSharedAccount(AddSharedAccountDto sharedAccountDto)
        {
            try
            {
                await _employeeService.AddSharedAccountAsync(sharedAccountDto.EmployeeId, sharedAccountDto.SharedAccountId, new string[] { sharedAccountDto.DeviceId });
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(sharedAccountDto.DeviceId);
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
        public async Task<IActionResult> UndoDeviceAccountChanges(string id)
        {
            try
            {
                await _employeeService.UndoChangesAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateWorkstationAccount(CreateWorkstationAccountDto workstationAccountDto)
        {
            IList<DeviceAccount> createdDeviceAccounts;
            try
            {
                var workstationAccount = new WorkstationAccount()
                {
                    Name = workstationAccountDto.Name,
                    AccountType = workstationAccountDto.AccountType,
                    Login = workstationAccountDto.Login,
                    Domain = workstationAccountDto.Domain,
                    Password = workstationAccountDto.Password
                };

                createdDeviceAccounts = await _employeeService.CreateWorkstationAccountAsync(workstationAccount, workstationAccountDto.EmployeeId, workstationAccountDto.DeviceId);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(workstationAccountDto.DeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetDeviceAccountById", new { id = createdDeviceAccounts[0].Id }, createdDeviceAccounts[0]);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> SetAsWorkstationAccount(SetAsWorkstationAccountDto workstationAccountDto)
        {
            try
            {
                await _employeeService.SetWorkstationAccountAsync(workstationAccountDto.DeviceId, workstationAccountDto.DeviceAccountId);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(workstationAccountDto.DeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        #endregion
    }
}