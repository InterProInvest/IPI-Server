using HES.Core.Entities;
using HES.Core.Exceptions;
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
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.AdminRole)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IAccountService _deviceAccountService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(IEmployeeService employeeService,
                                   IAccountService deviceAccountService,
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
            return await _employeeService.GetEmployeesAsync();
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
                await _employeeService.AddHardwareVaultAsync(deviceDto.EmployeeId, new string[] { deviceDto.DeviceId });
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
                await _employeeService.RemoveHardwareVaultAsync(deviceDto.EmployeeId, deviceDto.DeviceId, deviceDto.Reason);
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
        public async Task<ActionResult<IEnumerable<Account>>> GetAccountsByEmployeeId(string id)
        {
            return await _employeeService.GetAccountsByEmployeeIdAsync(id);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Account>> GetAccountById(string id)
        {
            var deviceAccount = await _employeeService.GetAccountByIdAsync(id);

            if (deviceAccount == null)
            {
                _logger.LogWarning($"{nameof(deviceAccount)} is null");
                return NotFound();
            }

            return deviceAccount;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<Account>> CreateAccount(CreateAccountDto accountDto)
        {
            Account createdAccount;
            try
            {
                var account = new Account()
                {
                    Name = accountDto.Name,
                    Urls = accountDto.Urls,
                    Apps = accountDto.Apps,
                    Login = accountDto.Login,
                    Kind = AccountKind.WebApp,
                    EmployeeId = accountDto.EmployeeId
                };
                var accountPassword = new AccountPassword()
                {
                    Password = accountDto.Password,
                    OtpSecret = accountDto.OtpSecret
                };

                createdAccount = await _employeeService.CreatePersonalAccountAsync(account, accountPassword);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(await _employeeService.GetEmployeeDevicesAsync(createdAccount.EmployeeId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetAccountById", new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditAccount(string id, EditAccountDto accountDto)
        {
            if (id != accountDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var account = new Account()
                {
                    Id = accountDto.Id,
                    EmployeeId = accountDto.EmployeeId,
                    Name = accountDto.Name,
                    Urls = accountDto.Urls,
                    Apps = accountDto.Apps,
                    Login = accountDto.Login,
                    Kind = AccountKind.WebApp
                };

                await _employeeService.EditPersonalAccountAsync(account);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(await _employeeService.GetEmployeeDevicesAsync(account.EmployeeId));
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
        public async Task<IActionResult> EditAccountPassword(string id, EditAccountPasswordDto accountDto)
        {
            if (id != accountDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var account = new Account()
                {
                    Id = accountDto.Id,
                    EmployeeId = accountDto.EmployeeId,
                    Kind = AccountKind.WebApp,
                };
                var accountPassword = new AccountPassword()
                {
                    Password = accountDto.Password
                };

                await _employeeService.EditPersonalAccountPwdAsync(account, accountPassword);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(await _employeeService.GetEmployeeDevicesAsync(account.EmployeeId));
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
        public async Task<IActionResult> EditAccountOtp(string id, EditAccountOtpDto accountDto)
        {
            if (id != accountDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var account = new Account()
                {
                    Id = accountDto.Id,
                    EmployeeId = accountDto.EmployeeId,
                    Kind = AccountKind.WebApp,
                };
                var accountPassword = new AccountPassword()
                {
                    OtpSecret = accountDto.OtpSercret
                };

                await _employeeService.EditPersonalAccountPwdAsync(account, accountPassword);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(await _employeeService.GetEmployeeDevicesAsync(account.EmployeeId));
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
        public async Task<ActionResult<Account>> DeleteAccount(string id)
        {
            Account account;
            try
            {
                account = await _employeeService.DeleteAccountAsync(id);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(account.Employee.HardwareVaults.Select(s => s.Id).ToList());
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return account;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddSharedAccount(AddSharedAccountDto sharedAccountDto)
        {
            Account createdAccount;
            try
            {
                createdAccount = await _employeeService.AddSharedAccountAsync(sharedAccountDto.EmployeeId, sharedAccountDto.SharedAccountId);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(await _employeeService.GetEmployeeDevicesAsync(createdAccount.EmployeeId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetAccountById", new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateWindowsLocalAccount(CreateWindowsAccountDto localAccountDto)
        {
            Account createdAccount;
            try
            {
                var workstationAccount = new WorkstationAccount()
                {
                    Name = localAccountDto.Name,
                    AccountType = WorkstationAccountType.Local,
                    Domain = "local",
                    Login = localAccountDto.Login,
                    Password = localAccountDto.Password
                };

                createdAccount = await _employeeService.CreateWorkstationAccountAsync(workstationAccount, localAccountDto.EmployeeId);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(await _employeeService.GetEmployeeDevicesAsync(createdAccount.EmployeeId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetAccountById", new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateWindowsDomainAccount(CreateWindowsDomainAccountDto domainAccountDto)
        {
            Account createdAccount;
            try
            {
                var workstationAccount = new WorkstationAccount()
                {
                    Name = domainAccountDto.Name,
                    AccountType = WorkstationAccountType.Domain,
                    Domain = domainAccountDto.Domain,
                    Login = domainAccountDto.Login,
                    Password = domainAccountDto.Password
                };

                createdAccount = await _employeeService.CreateWorkstationAccountAsync(workstationAccount, domainAccountDto.EmployeeId);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(await _employeeService.GetEmployeeDevicesAsync(createdAccount.EmployeeId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetDeviceAccountById", new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateWindowsMicrosoftAccount(CreateWindowsAccountDto microsoftAccountDto)
        {
            Account createdAccount;
            try
            {
                var workstationAccount = new WorkstationAccount()
                {
                    Name = microsoftAccountDto.Name,
                    AccountType = WorkstationAccountType.Microsoft,
                    Domain = "ms",
                    Login = microsoftAccountDto.Login,
                    Password = microsoftAccountDto.Password
                };

                createdAccount = await _employeeService.CreateWorkstationAccountAsync(workstationAccount, microsoftAccountDto.EmployeeId);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(await _employeeService.GetEmployeeDevicesAsync(createdAccount.EmployeeId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetDeviceAccountById", new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> SetAsWindowsAccount(SetAsWindowsAccountDto workstationAccountDto)
        {
            try
            {
                await _employeeService.SetAsWorkstationAccountAsync(workstationAccountDto.EmployeeId, workstationAccountDto.AccountId);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(await _employeeService.GetEmployeeDevicesAsync(workstationAccountDto.EmployeeId));
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