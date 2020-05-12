using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.DeviceAccessProfiles
{
    public class IndexModel : PageModel
    {
        private readonly IHardwareVaultService _deviceService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly ILogger<IndexModel> _logger;

        public IList<HardwareVaultProfile> DeviceAccessProfiles { get; set; }
        public HardwareVaultProfile DeviceAccessProfile { get; set; }
        public bool ProfileHasForeignKey { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(IHardwareVaultService deviceService,
                          IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                          ILogger<IndexModel> logger)
        {
            _deviceService = deviceService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            DeviceAccessProfiles = await _deviceService.GetProfilesAsync();
        }

        public IActionResult OnGetCreateProfile()
        {
            return Partial("_CreateProfile", this);
        }

        public async Task<IActionResult> OnPostCreateProfileAsync(HardwareVaultProfile deviceAccessProfile)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = Validation.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Index");
            }

            try
            {
                await _deviceService.CreateProfileAsync(deviceAccessProfile);
                SuccessMessage = $"Device profile created.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditProfileAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            DeviceAccessProfile = await _deviceService.GetProfileByIdAsync(id);

            if (DeviceAccessProfile == null)
            {
                _logger.LogWarning($"{nameof(DeviceAccessProfile)} is null");
                return NotFound();
            }

            return Partial("_EditProfile", this);
        }

        public async Task<IActionResult> OnPostEditProfileAsync(HardwareVaultProfile DeviceAccessProfile)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = Validation.GetModelStateErrors(ModelState);
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Index");
            }

            try
            {
                await _deviceService.EditProfileAsync(DeviceAccessProfile);               
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(await _deviceService.GetVaultIdsByProfileTaskAsync(DeviceAccessProfile.Id));
                SuccessMessage = $"Device access profile updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetDeleteProfileAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            DeviceAccessProfile = await _deviceService.GetProfileByIdAsync(id);

            if (DeviceAccessProfile == null)
            {
                _logger.LogWarning($"{nameof(DeviceAccessProfile)} is null");
                return NotFound();
            }

            ProfileHasForeignKey = DeviceAccessProfile.HardwareVaults.Count == 0 ? false : true;

            return Partial("_DeleteProfile", this);
        }

        public async Task<IActionResult> OnPostDeleteProfileAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            try
            {
                await _deviceService.DeleteProfileAsync(id);
                SuccessMessage = $"Device access profile deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetViewProfileAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning($"{nameof(id)} is null");
                return NotFound();
            }

            DeviceAccessProfile = await _deviceService.GetProfileByIdAsync(id);

            if (DeviceAccessProfile == null)
            {
                _logger.LogWarning($"{nameof(DeviceAccessProfile)} is null");
                return NotFound();
            }

            return Partial("_DetailsProfile", this);
        }
    }
}
