using HES.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using System.Net;
using System.Text.Encodings.Web;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppUsers;
using Microsoft.EntityFrameworkCore.Internal;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace HES.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class IdentityController : ControllerBase
    {
        private readonly UrlEncoder _urlEncoder;
        private readonly ILogger<IdentityController> _logger;
        private readonly IEmailSenderService _emailSenderService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IdentityController(UrlEncoder urlEncoder,
                                  ILogger<IdentityController> logger,
                                  IEmailSenderService emailSenderService,
                                  UserManager<ApplicationUser> userManager,
                                  SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _urlEncoder = urlEncoder;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSenderService = emailSenderService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApplicationUser>> GetUser()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
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
        public async Task<IActionResult> UpdateProfileInfo(ProfileInfo profileInfo)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    throw new Exception("User is null");

                if (profileInfo.Email != user.Email)
                {
                    var setEmailResult = await _userManager.SetEmailAsync(user, profileInfo.Email);

                    if (!setEmailResult.Succeeded)
                        throw new InvalidOperationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
                }

                if (profileInfo.PhoneNumber != user.PhoneNumber)
                {
                    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, profileInfo.PhoneNumber);

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendVerificationEmail()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    throw new Exception("User is null");

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var callbackUrl = $"{HttpContext.Request.PathBase}Identity/Account/ConfirmEmail?userId={user.Id}&code={WebUtility.UrlEncode(code)}";

                await _emailSenderService.SendUserConfirmEmailAsync(user.Email, HtmlEncoder.Default.Encode(callbackUrl));

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProfilePassword(ProfilePassword profilePassword)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    throw new Exception("User is null");

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, profilePassword.OldPassword, profilePassword.NewPassword);
                if (!changePasswordResult.Succeeded)
                    throw new Exception(string.Join(";", changePasswordResult.Errors));

                await _signInManager.RefreshSignInAsync(user);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TwoFactorInfo>> GetTwoFactorInfo()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    throw new Exception("User is null");

                var twoFactorInfo = new TwoFactorInfo
                {
                    HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
                    Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
                    IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
                    RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
                };

                return Ok(twoFactorInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgetTwoFactorClient()
        {
            try
            {
                await _signInManager.ForgetTwoFactorClientAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SharedKeyInfo>> LoadSharedKeyAndQrCodeUri()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    throw new Exception("User is null");

                var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
                if (string.IsNullOrWhiteSpace(unformattedKey))
                {
                    await _userManager.ResetAuthenticatorKeyAsync(user);
                    unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
                }

                var sharedKeyInfo = new SharedKeyInfo
                {
                    SharedKey = FormatKey(unformattedKey),
                    AuthenticatorUri = GenerateQrCodeUri(user.Email, unformattedKey)
                };

                return Ok(sharedKeyInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VerifyTwoFactorInfo>> VerifyTwoFactor(string inputCode)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    throw new Exception("User is null");

                var verificationCode = inputCode.Replace(" ", string.Empty).Replace("-", string.Empty);
                var isTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

                var verifyTwoFactorInfo = new VerifyTwoFactorInfo { IsTwoFactorTokenValid = isTokenValid };

                if (!isTokenValid)
                    return Ok(verifyTwoFactorInfo);

                await _userManager.SetTwoFactorEnabledAsync(user, true);

                _logger.LogInformation($"User with ID '{user.Id}' has enabled 2FA with an authenticator app.");

                if (await _userManager.CountRecoveryCodesAsync(user) == 0)
                {
                    var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                    verifyTwoFactorInfo.RecoveryCodes = recoveryCodes.ToList();

                    return Ok(verifyTwoFactorInfo);
                }

                return Ok(verifyTwoFactorInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

            return string.Format(
                AuthenticatorUriFormat,
                _urlEncoder.Encode("HES.Web"),
                _urlEncoder.Encode(email),
                unformattedKey);
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