using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HES.Core.Entities;
using HES.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using HES.Core.Interfaces;
using Microsoft.Extensions.Logging;
using HES.Core.Models.API;

namespace HES.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.AdminRole)]
    [Route("api/[controller]/[action]")]
    public class SharedAccountsController : ControllerBase
    {
        private readonly ISharedAccountService _sharedAccountService;
        private readonly ILogger<SharedAccountsController> _logger;

        public SharedAccountsController(ISharedAccountService sharedAccountService, ILogger<SharedAccountsController> logger)
        {
            _sharedAccountService = sharedAccountService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SharedAccount>>> GetSharedAccounts()
        {
            return await _sharedAccountService.GetSharedAccountsAsync();
        }

        [HttpGet("{id}")]
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
                return BadRequest(new { error = ex.Message });
            }

            return CreatedAtAction("GetSharedAccountById", new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPut("{id}")]
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

                await _sharedAccountService.EditSharedAccountAsync(sharedAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpPut("{id}")]
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

                await _sharedAccountService.EditSharedAccountAsync(sharedAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpPut("{id}")]
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

                await _sharedAccountService.EditSharedAccountOtpAsync(sharedAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<SharedAccount>> DeleteSharedAccount(string id)
        {
            var sharedAccount = await _sharedAccountService.GetByIdAsync(id);
            if (sharedAccount == null)
            {
                return NotFound();
            }

            try
            {
                await _sharedAccountService.DeleteSharedAccountAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            return sharedAccount;
        }
    }
}
