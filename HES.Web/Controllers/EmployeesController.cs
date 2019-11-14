using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models;
using HES.Core.Models.API;
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
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
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
                return BadRequest(new { error = ex.Message });
            }

            return CreatedAtAction("GetEmployeeById", new { id = createdEmployee.Id }, createdEmployee);
        }

        [HttpPut("{id}")]
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
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpDelete()]
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
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        #endregion

        #region Account

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<DeviceAccount>>> GetDeviceAccountsByEmployeeId(string id)
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
                return BadRequest(new { error = ex.Message });
            }

            return CreatedAtAction("GetDeviceAccountById", new { id = createdDeviceAccounts[0].Id }, createdDeviceAccounts[0]);
        }

        [HttpPut("{id}")]
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
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpPut("{id}")]
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
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpPut("{id}")]
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
                var deviceId = await _employeeService.DeleteAccount(id);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(deviceId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return deviceAccount;
        }

        [HttpPost]
        public async Task<IActionResult> AddSharedAccount(AddSharedAccountDto sharedAccountDto)
        {
            try
            {
                await _employeeService.AddSharedAccount(sharedAccountDto.EmployeeId, sharedAccountDto.SharedAccountId, new string[] { sharedAccountDto.DeviceId });
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(sharedAccountDto.DeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return Ok();
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> UndoDeviceAccountChanges(string id)
        {
            try
            {
                await _employeeService.UndoChanges(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkstationAccount(CreateWorkstationAccountDto workstationAccountDto)
        {
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

                await _employeeService.CreateWorkstationAccountAsync(workstationAccount, workstationAccountDto.EmployeeId, workstationAccountDto.DeviceId);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(workstationAccountDto.DeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> SetAsWorkstationAccount(SetAsWorkstationAccountDto workstationAccountDto)
        {
            try
            {
                await _employeeService.SetPrimaryAccount(workstationAccountDto.DeviceId, workstationAccountDto.DeviceAccountId);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(workstationAccountDto.DeviceId);
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