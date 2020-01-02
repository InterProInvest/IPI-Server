using HES.Core.Interfaces;
using HES.Core.Services;
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
        private readonly ILogger<IndexModel> _logger;

        public ProtectionStatus Status { get; set; }
        public NewPasswordModel NewPassword { get; set; }
        public CurrentPasswordModel CurrentPassword { get; set; }
        public ChangePasswordModel ChangePassword { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

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
        public class CurrentPasswordModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }
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

        public IndexModel(IDataProtectionService dataProtectionService, ILogger<IndexModel> logger)
        {
            _dataProtectionService = dataProtectionService;
            _logger = logger;
        }

        public void OnGet()
        {
            Status = _dataProtectionService.Status();
        }

        public async Task<IActionResult> OnPostEnableProtectionAsync(NewPasswordModel newPassword)
        {
            if (newPassword == null)
            {
                throw new Exception($"{nameof(newPassword)} is null");
            }

            try
            {
                await _dataProtectionService.EnableProtectionAsync(newPassword.Password);
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

        public async Task<IActionResult> OnPostDisableProtectionAsync(CurrentPasswordModel currentPassword)
        {
            if (currentPassword == null)
            {
                throw new Exception($"{nameof(currentPassword)} is null");
            }

            try
            {
                await _dataProtectionService.DisableProtectionAsync(currentPassword.Password);
                _logger.LogInformation($"Data protection disabled by {User.Identity.Name}");
                SuccessMessage = "Data protection disabled";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostChangeProtectionSecretAsync(ChangePasswordModel changePassword)
        {
            if (changePassword == null)
            {
                throw new Exception($"{nameof(changePassword)} is null");
            }

            try
            {
                await _dataProtectionService.ChangeProtectionPasswordAsync(changePassword.OldPassword, changePassword.NewPassword);
                _logger.LogInformation($"Data protection password changed by {User.Identity.Name}");
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