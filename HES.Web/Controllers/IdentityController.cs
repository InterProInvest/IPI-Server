using HES.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using HES.Core.Models.API.Identity;

namespace HES.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class IdentityController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<IdentityController> _logger;

        public IdentityController(SignInManager<ApplicationUser> signInManager,
                                  UserManager<ApplicationUser> userManager,
                                  ILogger<IdentityController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApplicationUser>> GetUser()
        {
            try
            {
                var user = await  _userManager.GetUserAsync(User);
                if (user == null)
                    throw new Exception("User is null");

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProfileInfo(ProfileInfoDto profileInfoDto)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    throw new Exception("User is null");

                if (profileInfoDto.Email != user.Email)
                {
                    var setEmailResult = await _userManager.SetEmailAsync(user, profileInfoDto.Email);

                    if (!setEmailResult.Succeeded)
                        throw new InvalidOperationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
                }

                if (profileInfoDto.PhoneNumber != user.PhoneNumber)
                {
                    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, profileInfoDto.PhoneNumber);

                    if (!setPhoneResult.Succeeded)
                        throw new InvalidOperationException($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.");
                }

                await _signInManager.RefreshSignInAsync(user);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(AuthDto authDto)
        {
            if (authDto == null)
            {
                _logger.LogWarning($"{nameof(authDto)} is null");
                return BadRequest(new { error = "CredentialsNull" });
            }

            // Find user
            var user = await _userManager.FindByEmailAsync(authDto.Email);
            if (user == null)
            {
                _logger.LogWarning($"User {authDto.Email} not found");
                return Unauthorized(new { error = "Unauthorized" });
            }

            // Verify password
            var passwordResult = await _signInManager.PasswordSignInAsync(authDto.Email, authDto.Password, false, lockoutOnFailure: true);
            if (passwordResult.Succeeded)
            {
                return Ok();
            }

            // Verify two factor
            if (passwordResult.RequiresTwoFactor)
            {
                if (string.IsNullOrWhiteSpace(authDto.Otp))
                {
                    return Unauthorized(new { error = "TwoFactorRequired" });
                }

                var authenticatorCode = authDto.Otp.Replace(" ", string.Empty).Replace("-", string.Empty);

                var twoFactorResult = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, false, false);

                if (twoFactorResult.Succeeded)
                {
                    return Ok();
                }
                else if (twoFactorResult.IsLockedOut)
                {
                    _logger.LogWarning($"User {user.Email} account locked out.");
                    return Unauthorized(new { error = "UserIsLockedout" });
                }
                else
                {
                    _logger.LogWarning($"Invalid authenticator code entered for user {user.Email}.");
                    return Unauthorized(new { error = "InvalidAuthenticatorCode" });
                }
            }

            // Is locked out
            if (passwordResult.IsLockedOut)
            {
                _logger.LogWarning($"User account {user.Email} locked out.");
                return Unauthorized(new { error = "UserIsLockedout" });
            }
            else
            {
                _logger.LogError($"User {user.Email} unauthorized.");
                return Unauthorized(new { error = "Unauthorized" });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> AuthN(AuthDto authDto)
        {
            if (authDto == null)
            {
                _logger.LogWarning($"{nameof(authDto)} is null");
                return BadRequest(new { error = "CredentialsNullException" });
            }

            // Find user
            var user = await _userManager.FindByEmailAsync(authDto.Email);
            if (user == null)
            {
                _logger.LogWarning($"User {authDto.Email} not found");
                return Unauthorized(new { error = "UserNotFoundException" });
            }

            // Verify password
            var passwordResult = await _userManager.CheckPasswordAsync(user, authDto.Password);
            if (!passwordResult)
            {
                _logger.LogWarning($"User {user.Email} verify password failed.");
                return Unauthorized(new { error = "UnauthorizedException" });
            }

            // Verify two factor
            if (user.TwoFactorEnabled)
            {
                if (string.IsNullOrWhiteSpace(authDto.Otp))
                {
                    return Unauthorized(new { error = "TwoFactorRequiredException" });
                }

                var authenticatorCode = authDto.Otp.Replace(" ", string.Empty).Replace("-", string.Empty);

                var tokenResult = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, authenticatorCode);
                if (!tokenResult)
                {
                    _logger.LogWarning($"User {authDto.Email} verify 2fa failed.");
                    return Unauthorized(new { error = "InvalidAuthenticatorCodeException" });
                }
            }

            return Ok(new User()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            });
        }
    }

    public class AuthDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Otp { get; set; }
    }

    class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}