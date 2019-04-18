﻿using HES.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartBreadcrumbs.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Administrators
{
    [Breadcrumb("Administrators")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        public IList<ApplicationUser> ApplicationUsers { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public ApplicationUser ApplicationUser { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task OnGetAsync()
        {
            ApplicationUsers = await _context.Users.ToListAsync();
        }

        #region Invite

        public IActionResult OnGetInviteAdmin()
        {
            return Partial("_Invite", this);
        }

        public async Task<IActionResult> OnPostInviteAdminAsync()
        {
            if (ModelState.IsValid)
            {
                // Create new user
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };
                var password = Guid.NewGuid().ToString();
                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    string errors = string.Empty;
                    foreach (var item in result.Errors)
                    {
                        errors += $"Error {Environment.NewLine} Code: {item.Code} Description: {item.Description} {Environment.NewLine}";
                    }
                    StatusMessage = errors;
                    return RedirectToPage("./Index");
                }
                await _userManager.AddToRoleAsync(user, ApplicationRoles.AdminRole);

                // Create "invite" link
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var email = Input.Email;
                var callbackUrl = Url.Page(
                   "/Account/Invite",
                    pageHandler: null,
                    values: new { area = "Identity", code, email },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Invite to HES",
                    $"Please enter your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                StatusMessage = "Email has been sent";
                return RedirectToPage("./Index");
            }
            return RedirectToPage("./Index");
        }

        #endregion

        #region Delete

        public async Task<IActionResult> OnGetDeleteAdminAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ApplicationUser = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);

            if (ApplicationUser == null)
            {
                return NotFound();
            }

            return Partial("_Delete", this);
        }

        public async Task<IActionResult> OnPostDeleteAdminAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ApplicationUser = await _context.Users.FindAsync(id);

            if (ApplicationUser != null)
            {
                _context.Users.Remove(ApplicationUser);
                await _context.SaveChangesAsync();
            }

            StatusMessage = "Removal was successful";
            return RedirectToPage("./Index");
        }

        #endregion
    }
}