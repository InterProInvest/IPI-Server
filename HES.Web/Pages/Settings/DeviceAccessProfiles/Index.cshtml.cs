using HES.Core.Entities;
using HES.Core.Interfaces;
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
        private readonly IDeviceService _deviceService;
        private readonly IDeviceAccessProfilesService _deviceAccessProfilesService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly ILogger<IndexModel> _logger;

        public IList<DeviceAccessProfile> DeviceAccessProfiles { get; set; }
        public DeviceAccessProfile DeviceAccessProfile { get; set; }
        public bool ProfileHasForeignKey { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(IDeviceService deviceService,
                          IDeviceAccessProfilesService deviceAccessProfilesService,
                          IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                          ILogger<IndexModel> logger)
        {
            _deviceService = deviceService;
            _deviceAccessProfilesService = deviceAccessProfilesService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            DeviceAccessProfiles = await _deviceAccessProfilesService
                .Query()
                .Include(d => d.Devices)
                .ToListAsync();
        }

        public IActionResult OnGetCreateProfile()
        {
            return Partial("_CreateProfile", this);
        }

        public async Task<IActionResult> OnPostCreateProfileAsync(DeviceAccessProfile DeviceAccessProfile)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values.SelectMany(s => s.Errors).Select(s => s.ErrorMessage).ToArray());
                ErrorMessage = errors;
                _logger.LogError($"DeviceAccessProfile. {errors}");
                return RedirectToPage("./Index");
            }

            try
            {
                await _deviceAccessProfilesService.CreateProfileAsync(DeviceAccessProfile);
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
                _logger.LogWarning("id == null");
                return NotFound();
            }

            DeviceAccessProfile = await _deviceAccessProfilesService
                .Query()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (DeviceAccessProfile == null)
            {
                _logger.LogWarning("DeviceAccessProfile == null");
                return NotFound();
            }

            return Partial("_EditProfile", this);
        }

        public async Task<IActionResult> OnPostEditProfileAsync(DeviceAccessProfile DeviceAccessProfile)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values.SelectMany(s => s.Errors).Select(s => s.ErrorMessage).ToArray());
                ErrorMessage = errors;
                _logger.LogError($"DeviceAccessProfile. {errors}");
                return RedirectToPage("./Index");
            }

            try
            {
                await _deviceAccessProfilesService.EditProfileAsync(DeviceAccessProfile);
                var devicesId = await _deviceService.UpdateProfileAsync(DeviceAccessProfile.Id);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devicesId);
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
                _logger.LogWarning("id == null");
                return NotFound();
            }

            DeviceAccessProfile = await _deviceAccessProfilesService
                .Query()
                .Include(d => d.Devices)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (DeviceAccessProfile == null)
            {
                _logger.LogWarning("DeviceAccessProfile == null");
                return NotFound();
            }

            ProfileHasForeignKey = DeviceAccessProfile.Devices.Count == 0 ? false : true;

            return Partial("_DeleteProfile", this);
        }

        public async Task<IActionResult> OnPostDeleteProfileAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            try
            {
                await _deviceAccessProfilesService.DeleteProfileAsync(id);
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
                _logger.LogWarning("id == null");
                return NotFound();
            }

            DeviceAccessProfile = await _deviceAccessProfilesService
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (DeviceAccessProfile == null)
            {
                _logger.LogWarning("DeviceAccessProfile == null");
                return NotFound();
            }

            return Partial("_DetailsProfile", this);
        }
    }
}
