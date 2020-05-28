using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.SharedAccounts;
using HES.Core.Models.API;
using HES.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using HES.Core.Enums;

namespace HES.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.AdminRole)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SharedAccountsController : ControllerBase
    {
        private readonly ISharedAccountService _sharedAccountService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly ILogger<SharedAccountsController> _logger;

        public SharedAccountsController(ISharedAccountService sharedAccountService,
                                        IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                                        ILogger<SharedAccountsController> logger)
        {
            _sharedAccountService = sharedAccountService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SharedAccount>>> GetSharedAccounts()
        {
            return await _sharedAccountService.GetSharedAccountsAsync();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SharedAccount>> GetSharedAccountById(string id)
        {
            var sharedAccount = await _sharedAccountService.GetByIdAsync(id);

            if (sharedAccount == null)
            {
                return NotFound();
            }

            return sharedAccount;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<SharedAccount>> CreateSharedAccount(CreateSharedAccountDto sharedAccountDto)
        {
            SharedAccount createdAccount;
            try
            {
                var sharedAccount = new SharedAccount()
                {
                    Name = sharedAccountDto.Name,
                    Urls = sharedAccountDto.Urls,
                    Apps = sharedAccountDto.Apps,
                    Login = sharedAccountDto.Login,
                    Password = sharedAccountDto.Password,
                    OtpSecret = sharedAccountDto.OtpSecret
                };

                createdAccount = await _sharedAccountService.CreateSharedAccountAsync(sharedAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetSharedAccountById", new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<SharedAccount>> CreateSharedWindowsLocalAccount(CreateWindowsAccountDto localAccountDto)
        {
            SharedAccount createdAccount;
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

                createdAccount = await _sharedAccountService.CreateWorkstationSharedAccountAsync(workstationAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetSharedAccountById", new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<SharedAccount>> CreateSharedWindowsDomainAccount(CreateWindowsDomainAccountDto domainAccountDto)
        {
            SharedAccount createdAccount;
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

                createdAccount = await _sharedAccountService.CreateWorkstationSharedAccountAsync(workstationAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetSharedAccountById", new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<SharedAccount>> CreateSharedWindowsMicrosoftAccount(CreateWindowsAccountDto workstationAccountDto)
        {
            SharedAccount createdAccount;
            try
            {
                var workstationAccount = new WorkstationAccount()
                {
                    Name = workstationAccountDto.Name,
                    AccountType = WorkstationAccountType.Microsoft,
                    Domain = "ms",
                    Login = workstationAccountDto.Login,
                    Password = workstationAccountDto.Password
                };

                createdAccount = await _sharedAccountService.CreateWorkstationSharedAccountAsync(workstationAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetSharedAccountById", new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditSharedAccount(string id, EditSharedAccountDto sharedAccountDto)
        {
            if (id != sharedAccountDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var sharedAccount = new SharedAccount()
                {
                    Id = sharedAccountDto.Id,
                    Name = sharedAccountDto.Name,
                    Urls = sharedAccountDto.Urls,
                    Apps = sharedAccountDto.Apps,
                    Login = sharedAccountDto.Login
                };

                var devices = await _sharedAccountService.EditSharedAccountAsync(sharedAccount);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devices);
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
        public async Task<IActionResult> EditSharedAccountPassword(string id, EditSharedAccountPasswordDto sharedAccountDto)
        {
            if (id != sharedAccountDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var sharedAccount = new SharedAccount()
                {
                    Id = sharedAccountDto.Id,
                    Password = sharedAccountDto.Password
                };

                var devices = await _sharedAccountService.EditSharedAccountPwdAsync(sharedAccount);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devices);
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
        public async Task<IActionResult> EditSharedAccountOtp(string id, EditSharedAccountOtpDto sharedAccountDto)
        {
            if (id != sharedAccountDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var sharedAccount = new SharedAccount()
                {
                    Id = sharedAccountDto.Id,
                    OtpSecret = sharedAccountDto.OtpSecret
                };

                var devices = await _sharedAccountService.EditSharedAccountOtpAsync(sharedAccount);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devices);
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
        public async Task<ActionResult<SharedAccount>> DeleteSharedAccount(string id)
        {
            var sharedAccount = await _sharedAccountService.GetByIdAsync(id);
            if (sharedAccount == null)
            {
                return NotFound();
            }

            try
            {
                var devices = await _sharedAccountService.DeleteSharedAccountAsync(id);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
            return sharedAccount;
        }
    }
}
