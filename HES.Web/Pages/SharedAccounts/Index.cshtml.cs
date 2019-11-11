using HES.Core.Entities;
using HES.Core.Models;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.SharedAccounts
{
    public class IndexModel : PageModel
    {
        private readonly ISharedAccountService _sharedAccountService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly ILogger<IndexModel> _logger;

        public IList<SharedAccount> SharedAccounts { get; set; }
        public SharedAccount SharedAccount { get; set; }
        public AccountPassword AccountPassword { get; set; }
        public WorkstationAccount WorkstationAccount { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(ISharedAccountService sharedAccountService,
                          IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                          ILogger<IndexModel> logger)
        {
            _sharedAccountService = sharedAccountService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            SharedAccounts = await _sharedAccountService
                .Query()
                .Where(d => d.Deleted == false)
                .ToListAsync();
        }

        #region Shared Account

        public IActionResult OnGetCreateSharedAccount()
        {
            ViewData["WorkstationAccountType"] = new SelectList(Enum.GetValues(typeof(WorkstationAccountType)).Cast<WorkstationAccountType>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            return Partial("_CreateSharedAccount", this);
        }

        public async Task<IActionResult> OnPostCreateSharedAccountAsync(SharedAccount sharedAccount, AccountPassword accountPassword)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values.SelectMany(s => s.Errors).Select(s => s.ErrorMessage).ToArray());
                _logger.LogError($"{errors}");
                ErrorMessage = errors;
                return RedirectToPage("./Index");
            }

            try
            {
                sharedAccount.Password = accountPassword.Password;
                sharedAccount.OtpSecret = accountPassword.OtpSecret;
                await _sharedAccountService.CreateSharedAccountAsync(sharedAccount);
                SuccessMessage = $"Shared account created.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostCreateWorkstationSharedAccountAsync(WorkstationAccount workstationAccount)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values.SelectMany(s => s.Errors).Select(s => s.ErrorMessage).ToArray());
                _logger.LogError($"{errors}");
                ErrorMessage = errors;
                return RedirectToPage("./Index");
            }

            try
            {
                await _sharedAccountService.CreateWorkstationSharedAccountAsync(workstationAccount);
                SuccessMessage = $"Shared account created.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditSharedAccountAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            SharedAccount = await _sharedAccountService
                .Query()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (SharedAccount == null)
            {
                _logger.LogWarning("SharedAccount == null");
                return NotFound();
            }

            return Partial("_EditSharedAccount", this);
        }

        public async Task<IActionResult> OnPostEditSharedAccountAsync(SharedAccount sharedAccount)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values.SelectMany(s => s.Errors).Select(s => s.ErrorMessage).ToArray());
                _logger.LogError($"{errors}");
                ErrorMessage = errors;
                return RedirectToPage("./Index");
            }

            try
            {
                var devices = await _sharedAccountService.EditSharedAccountAsync(sharedAccount);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devices);
                SuccessMessage = $"Shared account updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditSharedAccountPwdAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            SharedAccount = await _sharedAccountService
                .Query()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (SharedAccount == null)
            {
                _logger.LogWarning("SharedAccount == null");
                return NotFound();
            }
            return Partial("_EditSharedAccountPwd", this);
        }

        public async Task<IActionResult> OnPostEditSharedAccountPwdAsync(SharedAccount sharedAccount, AccountPassword accountPassword)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values.SelectMany(s => s.Errors).Select(s => s.ErrorMessage).ToArray());
                _logger.LogError($"{errors}");
                ErrorMessage = errors;
                return RedirectToPage("./Index");
            }

            try
            {
                sharedAccount.Password = accountPassword.Password;
                var devices = await _sharedAccountService.EditSharedAccountPwdAsync(sharedAccount);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devices);
                SuccessMessage = $"Shared account updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditSharedAccountOtpAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            SharedAccount = await _sharedAccountService
                .Query()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (SharedAccount == null)
            {
                _logger.LogWarning("SharedAccount == null");
                return NotFound();
            }

            return Partial("_EditSharedAccountOtp", this);
        }

        public async Task<IActionResult> OnPostEditSharedAccountOtpAsync(SharedAccount sharedAccount, AccountPassword accountPassword)
        {
            try
            {
                sharedAccount.OtpSecret = accountPassword.OtpSecret;
                var devices = await _sharedAccountService.EditSharedAccountOtpAsync(sharedAccount);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devices);
                SuccessMessage = $"Shared account updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetDeleteSharedAccountAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            SharedAccount = await _sharedAccountService
                .Query()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (SharedAccount == null)
            {
                _logger.LogWarning("SharedAccount == null");
                return NotFound();
            }
            return Partial("_DeleteSharedAccount", this);
        }

        public async Task<IActionResult> OnPostDeleteSharedAccountAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            try
            {
                var devices = await _sharedAccountService.DeleteSharedAccountAsync(id);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devices);
                SuccessMessage = $"Shared account deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        #endregion
    }
}