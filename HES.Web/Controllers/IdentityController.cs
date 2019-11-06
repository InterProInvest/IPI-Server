using HES.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]/[action]")]
    [ApiController]
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

        [HttpPost]
        public async Task<IActionResult> Login(AuthModel authModel)
        {
            if (authModel == null)
            {
                _logger.LogWarning("AuthModel is null");
                return BadRequest(new { error = "CredentialsNullException" });
            }

            // Find user
            var user = await _userManager.FindByEmailAsync(authModel.Email);
            if (user == null)
            {
                _logger.LogWarning($"[API] User {authModel.Email} not found");
                return Unauthorized(new { error = "UserNotFoundException" });
            }

            // Verify password
            var passwordResult = await _signInManager.PasswordSignInAsync(authModel.Email, authModel.Password, false, lockoutOnFailure: true);
            if (passwordResult.Succeeded)
            {
                _logger.LogInformation($"User {authModel.Email} succeeded auth via API");

                return Ok();
            }

            // Verify two factor
            if (passwordResult.RequiresTwoFactor)
            {
                if (string.IsNullOrWhiteSpace(authModel.Otp))
                {
                    return Unauthorized(new { error = "TwoFactorRequiredException" });
                }

                var authenticatorCode = authModel.Otp.Replace(" ", string.Empty).Replace("-", string.Empty);

                var twoFactorResult = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, false, false);

                if (twoFactorResult.Succeeded)
                {
                    _logger.LogInformation($"User {authModel.Email} succeeded auth via API with 2FA");

                    return Ok();
                }
                else if (twoFactorResult.IsLockedOut)
                {
                    _logger.LogWarning($"User {user.Email} account locked out.");
                    return Unauthorized(new { error = "UserIsLockedoutException" });
                }
                else
                {
                    _logger.LogWarning($"Invalid authenticator code entered for user {user.Email}.");
                    return Unauthorized(new { error = "InvalidAuthenticatorCodeException" });
                }
            }

            // Is locked out
            if (passwordResult.IsLockedOut)
            {
                _logger.LogWarning($"User account {user.Email} locked out.");
                return Unauthorized(new { error = "UserIsLockedoutException" });
            }
            else
            {
                _logger.LogError($"User {user.Email} unauthorized.");
                return Unauthorized(new { error = "UnauthorizedException" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AuthN(AuthModel authModel)
        {
            if (authModel == null)
            {
                _logger.LogWarning("[API] Credentials is null");
                return BadRequest(new { error = "CredentialsNullException" });
            }

            // Find user
            var user = await _userManager.FindByEmailAsync(authModel.Email);
            if (user == null)
            {
                _logger.LogWarning($"[API] User {authModel.Email} not found");
                return Unauthorized(new { error = "UserNotFoundException" });
            }

            //// Verify password
            //var passwordResult = await _signInManager.PasswordSignInAsync(data.Email, data.Password, false, lockoutOnFailure: true);
            //if (passwordResult.Succeeded)
            //{
            //    _logger.LogInformation($"User {data.Email} succeeded auth via API");

            //    await _signInManager.SignOutAsync();

            //    return Ok(new User()
            //    {
            //        FirstName = user.FirstName,
            //        LastName = user.LastName,
            //        Email = user.Email,
            //        PhoneNumber = user.PhoneNumber
            //    });
            //}

            //// Verify two factor
            //if (passwordResult.RequiresTwoFactor)
            //{
            //    if (string.IsNullOrWhiteSpace(data.Otp))
            //    {
            //        return Unauthorized(new { error = "TwoFactorRequiredException" });
            //    }

            //    var authenticatorCode = data.Otp.Replace(" ", string.Empty).Replace("-", string.Empty);

            //    var twoFactorResult = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, false, false);

            //    if (twoFactorResult.Succeeded)
            //    {
            //        _logger.LogInformation($"User {data.Email} succeeded auth via API with 2FA");

            //        await _signInManager.SignOutAsync();

            //        return Ok(new User()
            //        {
            //            FirstName = user.FirstName,
            //            LastName = user.LastName,
            //            Email = user.Email,
            //            PhoneNumber = user.PhoneNumber
            //        });
            //    }
            //    else if (twoFactorResult.IsLockedOut)
            //    {
            //        _logger.LogWarning($"User {user.Email} account locked out.");
            //        return Unauthorized(new { error = "UserIsLockedoutException" });
            //    }
            //    else
            //    {
            //        _logger.LogWarning($"Invalid authenticator code entered for user {user.Email}.");
            //        return Unauthorized(new { error = "InvalidAuthenticatorCodeException" });
            //    }
            //}

            //// Is locked out
            //if (passwordResult.IsLockedOut)
            //{
            //    _logger.LogWarning($"User account {user.Email} locked out.");
            //    return Unauthorized(new { error = "UserIsLockedoutException" });
            //}
            //else
            //{
            //    _logger.LogError($"User {user.Email} unauthorized.");
            //    return Unauthorized(new { error = "UnauthorizedException" });
            //}


            // Verify password
            var passwordResult = await _userManager.CheckPasswordAsync(user, authModel.Password);
            if (!passwordResult)
            {
                _logger.LogWarning($"[API] User {user.Email} verify password failed.");
                return Unauthorized(new { error = "UnauthorizedException" });
            }

            // Verify two factor
            if (user.TwoFactorEnabled)
            {
                if (string.IsNullOrWhiteSpace(authModel.Otp))
                {
                    return Unauthorized(new { error = "TwoFactorRequiredException" });
                }

                var authenticatorCode = authModel.Otp.Replace(" ", string.Empty).Replace("-", string.Empty);

                var tokenResult = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, authenticatorCode);
                if (!tokenResult)
                {
                    _logger.LogWarning($"[API] User {authModel.Email} verify 2fa failed.");
                    return Unauthorized(new { error = "InvalidAuthenticatorCodeException" });
                }
            }

            _logger.LogInformation($"[API] User {authModel.Email} verified successed");

            return Ok(new User()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            });
        }
    }

    public class AuthModel
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