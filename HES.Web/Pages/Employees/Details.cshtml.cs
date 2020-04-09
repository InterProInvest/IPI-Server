using HES.Core.Entities;
using HES.Core.Models;
using HES.Core.Interfaces;
using HES.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using HES.Core.Utilities;
using HES.Core.Enums;

namespace HES.Web.Pages.Employees
{
    public class DetailsModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDeviceService _deviceService;
        private readonly ISharedAccountService _sharedAccountService;
        private readonly ITemplateService _templateService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSenderService _emailSender;
        private readonly ILogger<DetailsModel> _logger;

        public IList<Device> Devices { get; set; }
        public IList<DeviceAccount> DeviceAccounts { get; set; }
        public IList<SharedAccount> SharedAccounts { get; set; }
        public WorkstationAccount WorkstationAccount { get; set; }


        public Device Device { get; set; }
        public Employee Employee { get; set; }
        public DeviceAccount DeviceAccount { get; set; }
        public SharedAccount SharedAccount { get; set; }
        public AccountPassword AccountPassword { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }
        [ViewData]
        public string EmployeeId { get; set; }
        [ViewData]
        public SelectList Templates { get; set; }
        [ViewData]
        public SelectList WorkstationAccountTypeList { get; set; }
        [ViewData]
        public SelectList SharedAccountIdList { get; set; }

        public DetailsModel(IEmployeeService employeeService,
                            IDeviceService deviceService,
                            ISharedAccountService sharedAccountService,
                            ITemplateService templateService,
                            IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                            UserManager<ApplicationUser> userManager,
                            IEmailSenderService emailSender,
                            ILogger<DetailsModel> logger)
        {
            _employeeService = employeeService;
            _deviceService = deviceService;
            _sharedAccountService = sharedAccountService;
            _templateService = templateService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (Employee == null)
            {
                _logger.LogWarning($"{nameof(Employee)} is null");
                return NotFound();
            }

            DeviceAccounts = await _employeeService.GetDeviceAccountsByEmployeeIdAsync(Employee.Id);

            ViewData["Devices"] = new SelectList(Employee.Devices.OrderBy(d => d.Id), "Id", "Id");
  
            return Page();
        }

        public async Task<IActionResult> OnGetUpdateTableAsync(string id)
        {
            Employee = await _employeeService.GetEmployeeByIdAsync(id);
            DeviceAccounts = await _employeeService.GetDeviceAccountsByEmployeeIdAsync(id);
            return Partial("_EmployeeDeviceAccounts", this);
        }

        #region Employee

        //public async Task<IActionResult> OnPostEnableSamlIdentityProviderAsync(Employee employee)
        //{
        //    try
        //    {
        //        // User
        //        var user = new ApplicationUser
        //        {
        //            UserName = employee.Email,
        //            Email = employee.Email,
        //            FirstName = employee.FirstName,
        //            LastName = employee.LastName,
        //            PhoneNumber = employee.PhoneNumber,
        //            DeviceId = employee.CurrentDevice
        //        };
        //        var password = Guid.NewGuid().ToString();
        //        var result = await _userManager.CreateAsync(user, password);
        //        if (!result.Succeeded)
        //        {
        //            var erorrs = string.Join("; ", result.Errors.Select(s => s.Description).ToArray());
        //            throw new Exception(erorrs);
        //        }

        //        // Role
        //        await _userManager.AddToRoleAsync(user, ApplicationRoles.UserRole);

        //        // Create link
        //        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        //        var email = employee.Email;
        //        var callbackUrl = Url.Page(
        //           "/Account/External/ActivateAccount",
        //            pageHandler: null,
        //            values: new { area = "Identity", code, email },
        //            protocol: Request.Scheme);

        //        await _emailSender.SendEmailAsync(email, "Hideez Enterpise Server - Activation of SAML IdP account",
        //            $"Dear {employee.FullName}, please click the link below to activate your SAML IdP account: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        //        SuccessMessage = "SAML IdP account enabled.";
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        ErrorMessage = ex.Message;
        //    }

        //    var id = employee.Id;
        //    return RedirectToPage("./Details", new { id });
        //}

        //public async Task<IActionResult> OnPostDisableSamlIdentityProviderAsync(Employee employee)
        //{
        //    try
        //    {
        //        var user = await _userManager.FindByEmailAsync(employee.Email);
        //        if (user == null)
        //        {
        //            throw new Exception("Email address does not exist.");
        //        }

        //        await _userManager.DeleteAsync(user);
        //        await _employeeService.DeleteSamlIdpAccountAsync(employee.Id);

        //        SuccessMessage = "SAML IdP account disabled.";
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        ErrorMessage = ex.Message;
        //    }

        //    var id = employee.Id;
        //    return RedirectToPage("./Details", new { id });
        //}

        //public async Task<IActionResult> OnPostResetSamlIdentityProviderAsync(Employee employee)
        //{
        //    try
        //    {
        //        var user = await _userManager.FindByEmailAsync(employee.Email);
        //        if (user == null)
        //        {
        //            throw new Exception("Email address does not exist.");
        //        }

        //        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        //        var email = employee.Email;
        //        var callbackUrl = Url.Page(
        //            "/Account/External/ResetAccountPassword",
        //            pageHandler: null,
        //            values: new { area = "Identity", code, email },
        //            protocol: Request.Scheme);

        //        await _emailSender.SendEmailAsync(
        //            email,
        //            "Hideez Enterpise Server - Reset Password of SAML IdP account",
        //            $"Dear {employee.FullName}, please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        //        SuccessMessage = "SAML IdP account password reseted.";
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        ErrorMessage = ex.Message;
        //    }

        //    var id = employee.Id;
        //    return RedirectToPage("./Details", new { id });
        //}

        private async Task<bool> EmployeeExists(string id)
        {
            return await _employeeService.ExistAsync(e => e.Id == id);
        }

        public async Task<IActionResult> OnGetSetPrimaryAccountAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            DeviceAccount = await _employeeService.GetDeviceAccountByIdAsync(id);

            if (DeviceAccount == null)
            {
                _logger.LogWarning($"{nameof(DeviceAccount)} is null");
                return NotFound();
            }

            return Partial("_SetPrimaryAccount", this);
        }

        public async Task<IActionResult> OnPostSetPrimaryAccountAsync(string deviceId, string accountId, string employeeId)
        {
            try
            {
                await _employeeService.SetAsWorkstationAccountAsync(deviceId, accountId);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(deviceId);
                SuccessMessage = "Windows account changed and will be recorded when the device is connected to the server.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            var id = employeeId;
            return RedirectToPage("./Details", new { id });
        }

        #endregion

        #region Device

        public async Task<IActionResult> OnGetAddDeviceAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (Employee == null)
            {
                _logger.LogWarning($"{nameof(Employee)} is null");
                return NotFound();
            }

            Devices = await _deviceService
                .DeviceQuery()
                .Where(d => d.EmployeeId == null && d.State == DeviceState.OK)
                .ToListAsync();

            return Partial("_AddDevice", this);
        }

        public async Task<IActionResult> OnPostAddDeviceAsync(string employeeId, string[] selectedDevices)
        {
            var id = employeeId;

            if (selectedDevices.Length <= 0)
            {
                ErrorMessage = "Device(s) was not selected";
                return RedirectToPage("./Details", new { id });
            }

            if (employeeId == null)
            {
                _logger.LogWarning($"{nameof(employeeId)} is null");
                return NotFound();
            }

            try
            {
                await _employeeService.AddDeviceAsync(employeeId, selectedDevices);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(selectedDevices);
                SuccessMessage = $"Device(s) added.";
            }
            catch (Exception ex)
            {
                if (!await EmployeeExists(employeeId))
                {
                    _logger.LogError("Employee dos not exists.");
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex.Message);
                    ErrorMessage = ex.Message;
                }
            }

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnGetDeleteDeviceAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            Device = await _deviceService.GetDeviceByIdAsync(id);

            if (Device == null)
            {
                _logger.LogWarning($"{nameof(Device)} is null");
                return NotFound();
            }

            return Partial("_DeleteDevice", this);
        }

        public async Task<IActionResult> OnPostDeleteDeviceAsync(Device device)
        {
            if (device == null)
            {
                _logger.LogWarning($"{nameof(device)} is null");
                return NotFound();
            }

            try
            {
                await _employeeService.RemoveDeviceAsync(device.Employee.Id, device.Id);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(device.Id);
                SuccessMessage = $"Device {device.Id} deleted.";
            }
            catch (Exception ex)
            {
                if (!await EmployeeExists(device.Employee.Id))
                {
                    _logger.LogError("Employee dos not exists.");
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex.Message);
                    ErrorMessage = ex.Message;
                }
            }

            var id = device.Employee.Id;
            return RedirectToPage("./Details", new { id });
        }

        #endregion

        #region Personal Account

        public async Task<JsonResult> OnGetJsonTemplateAsync(string id)
        {
            return new JsonResult(await _templateService.GetByIdAsync(id));
        }

        public async Task<IActionResult> OnGetCreatePersonalAccountAsync(string id)
        {
            EmployeeId = id;
            Templates = new SelectList(await _templateService.GetTemplatesAsync(), "Id", "Name");
            WorkstationAccountTypeList = new SelectList(Enum.GetValues(typeof(WorkstationAccountType)).Cast<WorkstationAccountType>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");

            Devices = await _deviceService.DeviceQuery().Where(d => d.EmployeeId == id).ToListAsync();

            return Partial("_CreatePersonalAccount", this);
        }

        public async Task<IActionResult> OnPostCreatePersonalAccountAsync(DeviceAccount deviceAccount, AccountPassword accountPassword, string[] selectedDevices)
        {
            var id = deviceAccount.EmployeeId;

            if (!ModelState.IsValid)
            {
                ErrorMessage = ValidationHepler.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Details", new { id });
            }

            try
            {
                await _employeeService.CreatePersonalAccountAsync(deviceAccount, accountPassword, selectedDevices);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(selectedDevices);
                SuccessMessage = "Account created and will be recorded when the device is connected to the server.";
            }
            catch (Exception ex)
            {
                if (!await EmployeeExists(id))
                {
                    _logger.LogError("Employee dos not exists.");
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex.Message);
                    ErrorMessage = ex.Message;
                }
            }

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnPostCreatePersonalWorkstationAccountAsync(WorkstationAccount workstationAccount, string employeeId, string[] selectedDevicesW)
        {
            var id = employeeId;

            if (!ModelState.IsValid)
            {
                ErrorMessage = ValidationHepler.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Details", new { id });
            }
            try
            {
                foreach (var deviceId in selectedDevicesW)
                {
                    await _employeeService.CreateWorkstationAccountAsync(workstationAccount, employeeId, deviceId);
                }

                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(selectedDevicesW);
                SuccessMessage = "Account created and will be recorded when the device is connected to the server.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnGetEditPersonalAccountAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            DeviceAccount = await _employeeService.GetDeviceAccountByIdAsync(id);

            if (DeviceAccount == null)
            {
                _logger.LogWarning($"{nameof(DeviceAccount)} is null");
                return NotFound();
            }

            return Partial("_EditPersonalAccount", this);
        }

        public async Task<IActionResult> OnPostEditPersonalAccountAsync(DeviceAccount deviceAccount)
        {
            var id = deviceAccount.EmployeeId;

            if (!ModelState.IsValid)
            {
                ErrorMessage = ValidationHepler.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Details", new { id });
            }

            try
            {
                await _employeeService.EditPersonalAccountAsync(deviceAccount);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(deviceAccount.DeviceId);
                SuccessMessage = "Account updated and will be recorded when the device is connected to the server.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnGetEditPersonalAccountPwdAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            DeviceAccount = await _employeeService.GetDeviceAccountByIdAsync(id);

            if (DeviceAccount == null)
            {
                _logger.LogWarning($"{nameof(DeviceAccount)} is null");
                return NotFound();
            }

            return Partial("_EditPersonalAccountPwd", this);
        }

        public async Task<IActionResult> OnPostEditPersonalAccountPwdAsync(DeviceAccount deviceAccount, AccountPassword accountPassword)
        {
            var id = deviceAccount.EmployeeId;

            if (!ModelState.IsValid)
            {
                ErrorMessage = ValidationHepler.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Details", new { id });
            }

            try
            {
                await _employeeService.EditPersonalAccountPwdAsync(deviceAccount, accountPassword);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(deviceAccount.DeviceId);
                SuccessMessage = "Account updated and will be recorded when the device is connected to the server.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnGetEditPersonalAccountOtpAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            DeviceAccount = await _employeeService.GetDeviceAccountByIdAsync(id);

            if (DeviceAccount == null)
            {
                _logger.LogWarning($"{nameof(DeviceAccount)} is null");
                return NotFound();
            }

            return Partial("_EditPersonalAccountOtp", this);
        }

        public async Task<IActionResult> OnPostEditPersonalAccountOtpAsync(DeviceAccount deviceAccount, AccountPassword accountPassword)
        {
            var id = deviceAccount.EmployeeId;

            try
            {
                await _employeeService.EditPersonalAccountOtpAsync(deviceAccount, accountPassword);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(deviceAccount.DeviceId);
                SuccessMessage = "Account updated and will be recorded when the device is connected to the server.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Details", new { id });
        }

        #endregion

        #region Shared Account

        public async Task<JsonResult> OnGetJsonSharedAccountAsync(string id)
        {
            return new JsonResult(await _sharedAccountService.GetByIdAsync(id));
        }

        public async Task<IActionResult> OnGetAddSharedAccountAsync(string id)
        {
            EmployeeId = id;
            SharedAccountIdList = new SelectList(await _sharedAccountService.GetSharedAccountsAsync(), "Id", "Name");

            SharedAccount = await _sharedAccountService.Query().FirstOrDefaultAsync(d => d.Deleted == false);
            Devices = await _deviceService.GetDevicesByEmployeeIdAsync(id);

            if (Devices == null)
            {
                _logger.LogWarning($"{nameof(Devices)} is null");
                return NotFound();
            }

            return Partial("_AddSharedAccount", this);
        }

        public async Task<IActionResult> OnPostAddSharedAccountAsync(string employeeId, string sharedAccountId, string[] selectedDevices)
        {
            if (employeeId == null)
            {
                _logger.LogWarning($"{nameof(employeeId)} is null");
                return NotFound();
            }

            try
            {
                await _employeeService.AddSharedAccountAsync(employeeId, sharedAccountId, selectedDevices);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(selectedDevices);
                SuccessMessage = "Account added and will be recorded when the device is connected to the server.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            var id = employeeId;
            return RedirectToPage("./Details", new { id });
        }

        #endregion

        #region Delete Account

        public async Task<IActionResult> OnGetDeleteAccountAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            DeviceAccount = await _employeeService.GetDeviceAccountByIdAsync(id);

            if (DeviceAccount == null)
            {
                _logger.LogWarning($"{nameof(DeviceAccount)} is null");
                return NotFound();
            }

            return Partial("_DeleteAccount", this);
        }

        public async Task<IActionResult> OnPostDeleteAccountAsync(string accountId, string employeeId)
        {
            if (accountId == null)
            {
                _logger.LogWarning($"{nameof(accountId)} is null");
                return NotFound();
            }

            try
            {
                var deviceId = await _employeeService.DeleteAccountAsync(accountId);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(deviceId);
                SuccessMessage = "Account deleting and will be deleted when the device is connected to the server.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            var id = employeeId;
            return RedirectToPage("./Details", new { id });
        }

        #endregion

        #region Undo

        public async Task<IActionResult> OnGetUndoChangesAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            DeviceAccount = await _employeeService.GetLastChangedAccountAsync(id);

            if (DeviceAccount == null)
            {
                _logger.LogWarning($"{nameof(DeviceAccount)} is null");
                return NotFound();
            }

            return Partial("_UndoChanges", this);
        }

        public async Task<IActionResult> OnPostUndoChangesAsync(string deviceId, string employeeId)
        {
            if (deviceId == null)
            {
                _logger.LogWarning($"{nameof(deviceId)} is null");
                return NotFound();
            }

            try
            {
                await _employeeService.UndoChangesAsync(deviceId);
                SuccessMessage = "Changes were canceled.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            var id = employeeId;
            return RedirectToPage("./Details", new { id });
        }

        #endregion
    }
}