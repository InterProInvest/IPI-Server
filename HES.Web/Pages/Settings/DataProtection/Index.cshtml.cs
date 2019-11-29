using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.DataProtection
{
    public class IndexModel : PageModel
    {
        private readonly IDataProtectionService _dataProtectionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<IndexModel> _logger;

        public ProtectionStatus Status { get; set; }
        public CurrentPasswordModel CurrentPassword { get; set; }
        public NewPasswordModel NewPassword { get; set; }
        public ChangePasswordModel ChangePassword { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public class CurrentPasswordModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }
        }

        public class NewPasswordModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public class ChangePasswordModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string OldPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public IndexModel(IDataProtectionService dataProtectionService, UserManager<ApplicationUser> userManager, ILogger<IndexModel> logger)
        {
            _dataProtectionService = dataProtectionService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            Status = await _dataProtectionService.Status();
        }

        public async Task<IActionResult> OnPostEnableProtectionAsync(NewPasswordModel NewPassword)
        {
            if (NewPassword == null)
            {
                _logger.LogWarning($"{nameof(NewPassword)} is null");
                return NotFound();
            }

            try
            {
                //var user = await _userManager.GetUserAsync(User);
                await _dataProtectionService.EnableProtectionAsync(NewPassword.Password);
                _logger.LogInformation($"Data protection enabled by {User.Identity.Name}");
                SuccessMessage = "Data protection enabled";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Index");
        }

        //public async Task<IActionResult> OnPostDisableDataProtectionAsync(CurrentPasswordModel CurrentPassword)
        //{
        //    if (CurrentPassword == null)
        //    {
        //        _logger.LogWarning("CurrentPassword == null");
        //        return NotFound();
        //    }

        //    try
        //    {
        //        var user = await _userManager.GetUserAsync(User);
        //        await _dataProtectionService.DisableDataProtectionAsync(CurrentPassword.Password, user.Email);
        //        SuccessMessage = "Data protection disabled";
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMessage = ex.Message;
        //        _logger.LogError(ex.Message);
        //    }

        //    return RedirectToPage("./Index");
        //}

        public async Task<IActionResult> OnPostActivateProtectionAsync(CurrentPasswordModel CurrentPassword)
        {
            if (CurrentPassword == null)
            {
                _logger.LogWarning($"{nameof(CurrentPassword)} is null");
                return NotFound();
            }

            try
            {
                //var user = await _userManager.GetUserAsync(User);
                await _dataProtectionService.ActivateProtectionAsync(CurrentPassword.Password);
                _logger.LogInformation($"Data protection enabled by {User.Identity.Name}");
                SuccessMessage = "Data protection activated";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostChangeProtectionSecretAsync(ChangePasswordModel ChangePassword)
        {
            if (ChangePassword == null)
            {
                _logger.LogWarning($"{nameof(ChangePassword)} is null");
                return NotFound();
            }

            try
            {
                //var user = await _userManager.GetUserAsync(User);
                await _dataProtectionService.ChangeProtectionSecretAsync(ChangePassword.OldPassword, ChangePassword.NewPassword);
                _logger.LogInformation($"Data protection enabled by {User.Identity.Name}");
                SuccessMessage = "Data protection password changed";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Index");
        }
    }
}