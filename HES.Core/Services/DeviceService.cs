using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models;
using HES.Core.Models.API.Device;
using HES.Core.Models.Web.HardwareVault;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Core.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IAsyncRepository<Device> _hardwareVaultRepository;
        private readonly IAsyncRepository<HardwareVaultActivation> _hardwareVaultActivationRepository;
        private readonly IAsyncRepository<DeviceAccessProfile> _deviceAccessProfileRepository;
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IHttpClientFactory _httpClientFactory;

        public DeviceService(IAsyncRepository<Device> deviceRepository,
                             IAsyncRepository<DeviceAccessProfile> deviceAccessProfileRepository,
                             IDeviceTaskService deviceTaskService,
                             IAppSettingsService appSettingsService,
                             IHttpClientFactory httpClientFactory)
        {
            _hardwareVaultRepository = deviceRepository;
            _deviceAccessProfileRepository = deviceAccessProfileRepository;
            _deviceTaskService = deviceTaskService;
            _appSettingsService = appSettingsService;
            _httpClientFactory = httpClientFactory;
        }

        #region Vault

        public IQueryable<Device> VaultQuery()
        {
            return _hardwareVaultRepository.Query();
        }

        public async Task<Device> GetDeviceByIdAsync(string id)
        {
            return await _hardwareVaultRepository
                .Query()
                .Include(d => d.Employee)
                .Include(d => d.DeviceAccessProfile)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Device>> GetDevicesByEmployeeIdAsync(string id)
        {
            return await _hardwareVaultRepository
                .Query()
                .Where(d => d.EmployeeId == id)
                .ToListAsync();
        }

        public async Task<List<Device>> GetDevicesAsync()
        {
            return await _hardwareVaultRepository
                .Query()
                .Include(d => d.DeviceAccessProfile)
                .Include(d => d.Employee.Department.Company)
                .ToListAsync();
        }

        public async Task<List<Device>> GetFilteredDevicesAsync(DeviceFilter deviceFilter)
        {
            var filter = _hardwareVaultRepository
                .Query()
                .Include(d => d.DeviceAccessProfile)
                .Include(c => c.Employee.Department.Company)
                .AsQueryable();

            if (deviceFilter.Battery != null)
            {
                filter = filter.Where(w => w.Battery == deviceFilter.Battery);
            }
            if (deviceFilter.Firmware != null)
            {
                filter = filter.Where(w => w.Firmware.Contains(deviceFilter.Firmware));
            }
            if (deviceFilter.LicenseStatus != null)
            {
                filter = filter.Where(w => w.LicenseStatus == deviceFilter.LicenseStatus);
            }
            if (deviceFilter.EmployeeId != null)
            {
                if (deviceFilter.EmployeeId == "N/A")
                {
                    filter = filter.Where(w => w.EmployeeId == null);
                }
                else
                {
                    filter = filter.Where(w => w.EmployeeId == deviceFilter.EmployeeId);
                }
            }
            if (deviceFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Employee.Department.Company.Id == deviceFilter.CompanyId);
            }
            if (deviceFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.Employee.DepartmentId == deviceFilter.DepartmentId);
            }
            if (deviceFilter.StartDate != null && deviceFilter.EndDate != null)
            {
                filter = filter.Where(w => w.LastSynced.HasValue
                                        && w.LastSynced.Value >= deviceFilter.StartDate.Value.AddSeconds(0).AddMilliseconds(0).ToUniversalTime()
                                        && w.LastSynced.Value <= deviceFilter.EndDate.Value.AddSeconds(59).AddMilliseconds(999).ToUniversalTime());
            }

            return await filter
                .OrderBy(w => w.Id)
                .Take(deviceFilter.Records)
                .ToListAsync();
        }

        public async Task<List<Device>> GetVaultsAsync(int skip, int take, string sortColumn, ListSortDirection sortDirection, string searchText, HardwareVaultFilter filter)
        {
            return await GetDevicesAsync();
        }

        public async Task<int> GetVaultsCountAsync(string searchText, HardwareVaultFilter filter)
        {
            return await _hardwareVaultRepository.Query().CountAsync();
        }

        public async Task<Device> AddDeviceAsync(Device device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            return await _hardwareVaultRepository.AddAsync(device);
        }

        public async Task ImportDevicesAsync()
        {
            var licensing = await _appSettingsService.GetLicensingSettingsAsync();

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(licensing.ApiAddress);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var path = $"api/Devices/GetDevices/{licensing.ApiKey}";
            var response = await client.GetAsync(path);

            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var newDevicesDto = JsonConvert.DeserializeObject<List<DeviceImportDto>>(data);

                var currentDevices = await GetDevicesAsync();
                var devicesToImport = new List<Device>();
                newDevicesDto.RemoveAll(x => currentDevices.Select(s => s.Id).Contains(x.DeviceId));

                foreach (var newDeviceDto in newDevicesDto)
                {
                    devicesToImport.Add(new Device()
                    {
                        Id = newDeviceDto.DeviceId,
                        MAC = newDeviceDto.MAC,
                        Model = newDeviceDto.Model,
                        RFID = newDeviceDto.RFID,
                        Battery = 100,
                        Firmware = newDeviceDto.Firmware,
                        Status = VaultStatus.Ready,
                        StatusReason = VaultStatusReason.None,
                        StatusDescription = null,
                        LastSynced = null,
                        NeedSync = false,
                        EmployeeId = null,
                        MasterPassword = null,
                        AcceessProfileId = ServerConstants.DefaulAccessProfileId,
                        ImportedAt = DateTime.UtcNow,
                        HasNewLicense = false,
                        LicenseStatus = LicenseStatus.None,
                        LicenseEndDate = null
                    });
                }

                await _hardwareVaultRepository.AddRangeAsync(devicesToImport);
            }
        }

        public async Task EditRfidAsync(Device device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            await _hardwareVaultRepository.UpdateOnlyPropAsync(device, new string[] { "RFID" });
        }

        public async Task UpdateOnlyPropAsync(Device device, string[] properties)
        {
            await _hardwareVaultRepository.UpdateOnlyPropAsync(device, properties);
        }

        public async Task UpdateDeviceInfoAsync(string vaultId, int battery, string firmware, bool locked)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            var vault = await _hardwareVaultRepository.GetByIdAsync(vaultId);
            if (vault == null)
                throw new Exception($"Vault not found, ID: {vaultId}");

            vault.Battery = battery;
            vault.Firmware = firmware;

            if (vault.Status == VaultStatus.Active && locked) // TODO move this to AppHub.SetLocked
            {
                vault.Status = VaultStatus.Deactivated;
            }

            vault.LastSynced = DateTime.UtcNow;

            await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, new string[] { nameof(Device.Battery), nameof(Device.Firmware), nameof(Device.Status), nameof(Device.LastSynced) });
        }

        public async Task UpdateNeedSyncAsync(Device device, bool needSync)
        {
            device.NeedSync = needSync;
            await _hardwareVaultRepository.UpdateOnlyPropAsync(device, new string[] { nameof(Device.NeedSync) });
        }

        public async Task UpdateNeedSyncAsync(IList<Device> devices, bool needSync)
        {
            foreach (var device in devices)
            {
                device.NeedSync = needSync;
            }
            await _hardwareVaultRepository.UpdateOnlyPropAsync(devices, new string[] { nameof(Device.NeedSync) });
        }

        public async Task UnlockPinAsync(string deviceId)
        {
            if (deviceId == null)
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            var device = await _hardwareVaultRepository.GetByIdAsync(deviceId);
            if (device == null)
            {
                throw new Exception($"Device not found, ID: {deviceId}");
            }

            throw new NotImplementedException(); //TODO
            // Update device state
            //device.Status = DeviceState.PendingUnlock;
            await _hardwareVaultRepository.UpdateOnlyPropAsync(device, new string[] { "State" });

            // Create task
            await _deviceTaskService.AddUnlockPinAsync(device);
        }

        public async Task<bool> ExistAsync(Expression<Func<Device, bool>> predicate)
        {
            return await _hardwareVaultRepository.ExistAsync(predicate);
        }

        public async Task RemoveEmployeeAsync(string deviceId)
        {
            var device = await _hardwareVaultRepository.GetByIdAsync(deviceId);

            device.EmployeeId = null;
            device.MasterPassword = null;
            device.AcceessProfileId = "default";
            device.LastSynced = DateTime.UtcNow;
            device.NeedSync = false;

            var properties = new List<string>()
            {
                nameof(Device.EmployeeId),
                nameof(Device.MasterPassword),
                nameof(Device.AcceessProfileId),
                nameof(Device.LastSynced),
                nameof(Device.NeedSync)
            };

            await _hardwareVaultRepository.UpdateOnlyPropAsync(device, properties.ToArray());
        }

        //public async Task RestoreDefaultsAsync(string deviceId)
        //{
        //    var device = await _deviceRepository.GetByIdAsync(deviceId);

        //    device.LastSynced = DateTime.UtcNow;
        //    device.AcceessProfileId = "default";
        //    device.Status = DeviceState.OK;

        //    var properties = new List<string>()
        //    {
        //        "LastSynced",
        //        "AcceessProfileId",
        //        "State"
        //    };

        //    await _deviceRepository.UpdateOnlyPropAsync(device, properties.ToArray());
        //}

        public async Task SetVaultStatusAsync(string vaultId, VaultStatus vaultStatus)
        {
            var vault = await GetDeviceByIdAsync(vaultId);

            if (vault.Status == vaultStatus)
                return;

            vault.Status = vaultStatus;
            await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, new string[] { nameof(Device.Status) });
        }

        #endregion

        #region Vault Activation

        public async Task GenerateVaultActivationAsync(string vaultId)
        {
            var vaultActivation = new HardwareVaultActivation()
            {
                DeviceId = vaultId,
                AcivationCode = new Random().Next(100000, 999999).ToString(),
                WrongAttemptsCount = 0,
                LastWrongAttempt = null,
                CreatedAt = DateTime.UtcNow,
                Status = HardwareVaultActivationStatus.Pending
            };
            await _hardwareVaultActivationRepository.AddAsync(vaultActivation);
        }

        public async Task<HardwareVaultActivation> GetVaultActivationAsync(string vaultId)
        {
            return await _hardwareVaultActivationRepository.Query().FirstOrDefaultAsync(x => x.DeviceId == vaultId);
        }

        #endregion

        #region Profile

        public IQueryable<DeviceAccessProfile> AccessProfileQuery()
        {
            return _deviceAccessProfileRepository.Query();
        }

        public async Task<DeviceAccessProfile> GetAccessProfileByIdAsync(string id)
        {
            return await _deviceAccessProfileRepository
                .Query()
                .Include(d => d.Devices)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<DeviceAccessProfile>> GetAccessProfilesAsync()
        {
            return await _deviceAccessProfileRepository
                .Query()
                .Include(d => d.Devices)
                .ToListAsync();
        }

        public async Task<DeviceAccessProfile> CreateProfileAsync(DeviceAccessProfile deviceAccessProfile)
        {
            if (deviceAccessProfile == null)
            {
                throw new ArgumentNullException(nameof(deviceAccessProfile));
            }

            var profile = await _deviceAccessProfileRepository
                .Query()
                .Where(d => d.Name == deviceAccessProfile.Name)
                .AnyAsync();

            if (profile)
            {
                throw new Exception($"Name {deviceAccessProfile.Name} is already taken.");
            }

            deviceAccessProfile.CreatedAt = DateTime.UtcNow;
            return await _deviceAccessProfileRepository.AddAsync(deviceAccessProfile);
        }

        public async Task EditProfileAsync(DeviceAccessProfile deviceAccessProfile)
        {
            if (deviceAccessProfile == null)
            {
                throw new ArgumentNullException(nameof(deviceAccessProfile));
            }

            var profile = await _deviceAccessProfileRepository
               .Query()
               .Where(d => d.Name == deviceAccessProfile.Name && d.Id != deviceAccessProfile.Id)
               .AnyAsync();

            if (profile)
            {
                throw new Exception($"Name {deviceAccessProfile.Name} is already taken.");
            }

            deviceAccessProfile.UpdatedAt = DateTime.UtcNow;
            await _deviceAccessProfileRepository.UpdateAsync(deviceAccessProfile);
        }

        public async Task DeleteProfileAsync(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (id == "default")
            {
                throw new Exception("Cannot delete a default profile");
            }

            var deviceAccessProfile = await _deviceAccessProfileRepository.GetByIdAsync(id);
            if (deviceAccessProfile == null)
            {
                throw new Exception("Device access profile not found");
            }

            await _deviceAccessProfileRepository.DeleteAsync(deviceAccessProfile);
        }

        public async Task SetProfileAsync(string[] devicesId, string profileId)
        {
            if (devicesId == null)
            {
                throw new ArgumentNullException(nameof(devicesId));
            }
            if (profileId == null)
            {
                throw new ArgumentNullException(nameof(profileId));
            }

            var state = await _hardwareVaultRepository.Query().Where(x => devicesId.Contains(x.Id) && x.Status != VaultStatus.Active).AsNoTracking().AnyAsync();
            if (state)
            {
                throw new Exception("You have chosen a device with a status that does not allow changing the profile.");
            }

            var profile = await _deviceAccessProfileRepository.GetByIdAsync(profileId);
            if (profile == null)
            {
                throw new Exception("Profile not found");
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var deviceId in devicesId)
                {
                    var device = await _hardwareVaultRepository.GetByIdAsync(deviceId);
                    if (device != null)
                    {
                        device.AcceessProfileId = profileId;
                        await _hardwareVaultRepository.UpdateOnlyPropAsync(device, new string[] { "AcceessProfileId" });

                        if (device.MasterPassword != null && device.EmployeeId != null)
                        {
                            // Delete all previous tasks for update profile
                            await _deviceTaskService.RemoveAllProfileTasksAsync(device.Id);
                            // Add task for update profile
                            await _deviceTaskService.AddProfileAsync(device);
                        }
                    }
                }
                transactionScope.Complete();
            }
        }

        public async Task<string[]> UpdateProfileAsync(string profileId)
        {
            // Get devices by profile id
            var tasks = await _deviceTaskService
               .TaskQuery()
               .Where(d => d.Operation == TaskOperation.Wipe || d.Operation == TaskOperation.Link)
               .Select(s => s.DeviceId)
               .AsNoTracking()
               .ToListAsync();

            var devicesIds = await _hardwareVaultRepository
               .Query()
               .Where(d => d.AcceessProfileId == profileId && d.EmployeeId != null && !tasks.Contains(d.Id))
               .Select(s => s.Id)
               .AsNoTracking()
               .ToArrayAsync();

            if (devicesIds.Length > 0)
            {
                await SetProfileAsync(devicesIds, profileId);
            }

            return devicesIds;
        }

        #endregion
    }
}
