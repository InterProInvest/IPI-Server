using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSenderService _emailSender;
        private readonly IEmployeeService _employeeService;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager,
                                   IEmailSenderService emailSender,
                                   IEmployeeService employeeService)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _employeeService = employeeService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null /*|| !(await _userManager.IsEmailConfirmedAsync(user))*/)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }
                var employee = await _employeeService.Query().FirstOrDefaultAsync(e => e.Email == Input.Email);
                var resetPasswordUrl = "/Account/ResetPassword";
                var emailTitle = "Reset Password";
                var emailBody = "Please reset your password by";

                var role = await _userManager.IsInRoleAsync(user, ApplicationRoles.UserRole);
                if (role)
                {
                    resetPasswordUrl = "/Account/External/ResetAccountPassword";
                    emailTitle = "Hideez Enterpise Server - Reset Password of SAML IdP account";
                    emailBody = $"Dear {employee.FullName}, please reset your password by";
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var email = Input.Email;
                var callbackUrl = Url.Page(
                    resetPasswordUrl,
                    pageHandler: null,
                    values: new { code, email },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    emailTitle,
                    $"{emailBody} <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}