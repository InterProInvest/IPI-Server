using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
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
        private readonly IAsyncRepository<DeviceAccessProfile> _hardwareVaultProfileRepository;
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly IAccountService _accountService;
        private readonly IWorkstationService _workstationService;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly IHttpClientFactory _httpClientFactory;

        public DeviceService(IAsyncRepository<Device> hardwareVaultRepository,
                             IAsyncRepository<HardwareVaultActivation> hardwareVaultActivationRepository,
                             IAsyncRepository<DeviceAccessProfile> deviceAccessProfileRepository,
                             IDeviceTaskService deviceTaskService,
                             IAccountService accountService,
                             IWorkstationService workstationService,
                             IAppSettingsService appSettingsService,
                             IDataProtectionService dataProtectionService,
                             IHttpClientFactory httpClientFactory)
        {
            _hardwareVaultRepository = hardwareVaultRepository;
            _hardwareVaultActivationRepository = hardwareVaultActivationRepository;
            _hardwareVaultProfileRepository = deviceAccessProfileRepository;
            _deviceTaskService = deviceTaskService;
            _accountService = accountService;
            _workstationService = workstationService;
            _appSettingsService = appSettingsService;
            _dataProtectionService = dataProtectionService;
            _httpClientFactory = httpClientFactory;
        }

        #region Vault

        public IQueryable<Device> VaultQuery()
        {
            return _hardwareVaultRepository.Query();
        }

        public async Task<Device> GetVaultByIdAsync(string profileId)
        {
            return await _hardwareVaultRepository
                .Query()
                .Include(d => d.Employee)
                .Include(d => d.DeviceAccessProfile)
                .FirstOrDefaultAsync(m => m.Id == profileId);
        }

        public async Task<List<Device>> GetVaultsByEmployeeIdAsync(string profileId)
        {
            return await _hardwareVaultRepository
                .Query()
                .Where(d => d.EmployeeId == profileId)
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
            var query = _hardwareVaultRepository
                .Query()
                .Include(d => d.DeviceAccessProfile)
                .Include(d => d.Employee.Department)
                .Include(d => d.Employee.Department.Company)
                .AsQueryable();

            // Filter
            if (filter != null)
            {
                if (filter.Battery != null)
                {
                    query = query.Where(w => w.Battery == filter.Battery);
                }
                if (filter.Firmware != null)
                {
                    query = query.Where(w => w.Firmware.Contains(filter.Firmware));
                }
                if (filter.LicenseStatus != null)
                {
                    query = query.Where(w => w.LicenseStatus == filter.LicenseStatus);
                }
                if (filter.EmployeeName != null)
                {
                    query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(filter.EmployeeName, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.CompanyName != null)
                {
                    query = query.Where(w => w.Employee.Department.Company.Name.Contains(filter.CompanyName, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.DepartmentName != null)
                {
                    query = query.Where(w => w.Employee.Department.Name.Contains(filter.DepartmentName, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.StartDate != null && filter.EndDate != null)
                {
                    query = query.Where(w => w.LastSynced.HasValue
                                            && w.LastSynced.Value >= filter.StartDate.Value.AddSeconds(0).AddMilliseconds(0).ToUniversalTime()
                                            && w.LastSynced.Value <= filter.EndDate.Value.AddSeconds(59).AddMilliseconds(999).ToUniversalTime());
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim();

                query = query.Where(x => x.Id.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.MAC.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Battery.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Firmware.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.DeviceAccessProfile.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.LastSynced.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.LicenseEndDate.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    (x.Employee.FirstName + " " + x.Employee.LastName).Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Company.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Model.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.RFID.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            // Sort Direction
            switch (sortColumn)
            {
                case nameof(Device.Id):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id);
                    break;
                case nameof(Device.MAC):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.MAC) : query.OrderByDescending(x => x.MAC);
                    break;
                case nameof(Device.Battery):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Battery) : query.OrderByDescending(x => x.Battery);
                    break;
                case nameof(Device.Firmware):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Firmware) : query.OrderByDescending(x => x.Firmware);
                    break;
                case nameof(Device.DeviceAccessProfile.Name):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.DeviceAccessProfile.Name) : query.OrderByDescending(x => x.DeviceAccessProfile.Name);
                    break;
                case nameof(Device.Status):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status);
                    break;
                case nameof(Device.LastSynced):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.LastSynced) : query.OrderByDescending(x => x.LastSynced);
                    break;
                case nameof(Device.LicenseStatus):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.LicenseStatus) : query.OrderByDescending(x => x.LicenseStatus);
                    break;
                case nameof(Device.LicenseEndDate):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.LicenseEndDate) : query.OrderByDescending(x => x.LicenseEndDate);
                    break;
                case nameof(Device.Employee.FullName):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.FirstName).ThenBy(x => x.Employee.LastName) : query.OrderByDescending(x => x.Employee.FirstName).ThenByDescending(x => x.Employee.LastName);
                    break;
                case nameof(Device.Employee.EmpCompany):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.Department.Company) : query.OrderByDescending(x => x.Employee.Department.Company);
                    break;
                case nameof(Device.Model):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Model) : query.OrderByDescending(x => x.Model);
                    break;
                case nameof(Device.Employee.Department):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.Department.Name) : query.OrderByDescending(x => x.Employee.Department.Name);
                    break;
                case nameof(Device.RFID):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.RFID) : query.OrderByDescending(x => x.RFID);
                    break;
            }

            return await query.Skip(skip).Take(take).ToListAsync();
        }

        public async Task<int> GetVaultsCountAsync(string searchText, HardwareVaultFilter filter)
        {
            var query = _hardwareVaultRepository
                .Query()
                .Include(d => d.DeviceAccessProfile)
                .Include(d => d.Employee.Department)
                .Include(d => d.Employee.Department.Company)
                .AsQueryable();


            // Filter
            if (filter != null)
            {
                if (filter.Battery != null)
                {
                    query = query.Where(w => w.Battery == filter.Battery);
                }
                if (filter.Firmware != null)
                {
                    query = query.Where(w => w.Firmware.Contains(filter.Firmware));
                }
                if (filter.LicenseStatus != null)
                {
                    query = query.Where(w => w.LicenseStatus == filter.LicenseStatus);
                }
                if (filter.EmployeeName != null)
                {
                    query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(filter.EmployeeName, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.CompanyName != null)
                {
                    query = query.Where(w => w.Employee.Department.Company.Name.Contains(filter.CompanyName, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.DepartmentName != null)
                {
                    query = query.Where(w => w.Employee.Department.Name.Contains(filter.DepartmentName, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.StartDate != null && filter.EndDate != null)
                {
                    query = query.Where(w => w.LastSynced.HasValue
                                            && w.LastSynced.Value >= filter.StartDate.Value.AddSeconds(0).AddMilliseconds(0).ToUniversalTime()
                                            && w.LastSynced.Value <= filter.EndDate.Value.AddSeconds(59).AddMilliseconds(999).ToUniversalTime());
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim();

                query = query.Where(x => x.Id.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.MAC.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Battery.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Firmware.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.DeviceAccessProfile.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.LastSynced.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.LicenseEndDate.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    (x.Employee.FirstName + " " + x.Employee.LastName).Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Company.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Model.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.RFID.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            return await query.CountAsync();
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
                var devicesDto = JsonConvert.DeserializeObject<List<DeviceImportDto>>(data);

                var devicesCount = await GetVaultsCountAsync(string.Empty, null);
                var devices = await GetVaultsAsync(0, devicesCount, nameof(Device.Id), ListSortDirection.Ascending, string.Empty, null);
                var devicesToImport = new List<Device>();
                devicesDto.RemoveAll(x => devices.Select(s => s.Id).Contains(x.DeviceId));

                foreach (var deviceDto in devicesDto)
                {
                    devicesToImport.Add(new Device()
                    {
                        Id = deviceDto.DeviceId,
                        MAC = deviceDto.MAC,
                        Model = deviceDto.Model,
                        RFID = deviceDto.RFID,
                        Battery = 100,
                        Firmware = deviceDto.Firmware,
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
                        LicenseStatus = VaultLicenseStatus.None,
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

        public Task UnchangedVaultAsync(Device vault)
        {
            return _hardwareVaultRepository.Unchanged(vault);
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

        public async Task<bool> ExistAsync(Expression<Func<Device, bool>> predicate)
        {
            return await _hardwareVaultRepository.ExistAsync(predicate);
        }

        public async Task<HardwareVaultActivation> GenerateVaultActivationAsync(string vaultId)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            var vaultActivation = new HardwareVaultActivation()
            {
                VaultId = vaultId,
                AcivationCode = _dataProtectionService.Encrypt(new Random().Next(100000, 999999).ToString()),
                CreatedAt = DateTime.UtcNow,
                Status = HardwareVaultActivationStatus.Pending
            };

            return await _hardwareVaultActivationRepository.AddAsync(vaultActivation);
        }

        public async Task ChangeVaultActivationStatusAsync(string vaultId, HardwareVaultActivationStatus status)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            _dataProtectionService.Validate();

            var vaultActivation = await _hardwareVaultActivationRepository
                .Query()
                .FirstOrDefaultAsync(x => x.VaultId == vaultId && x.Status == HardwareVaultActivationStatus.Pending);

            if (vaultActivation != null)
            {
                vaultActivation.Status = status;
                await _hardwareVaultActivationRepository.UpdateOnlyPropAsync(vaultActivation, new string[] { nameof(HardwareVaultActivation.Status) });
            }
        }

        public async Task<string> GetVaultActivationCodeAsync(string vaultId)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            _dataProtectionService.Validate();

            var vaultActivation = await _hardwareVaultActivationRepository.Query().FirstOrDefaultAsync(x => x.VaultId == vaultId && x.Status == HardwareVaultActivationStatus.Pending);
            return _dataProtectionService.Decrypt(vaultActivation.AcivationCode);
        }

        public async Task ActivateVaultAsync(string vaultId)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            var vault = await GetVaultByIdAsync(vaultId);
            if (vault == null)
                throw new Exception($"Vault {vaultId} not found");

            if (vault.Status != VaultStatus.Locked)
                throw new Exception($"Vault {vaultId} status ({vault.Status}) is not allowed to execute this operation");

            vault.Status = VaultStatus.Suspended;
            vault.StatusReason = VaultStatusReason.None;
            vault.StatusDescription = "Pending hardware vault activation";

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await GenerateVaultActivationAsync(vaultId);
                await UpdateOnlyPropAsync(vault, new string[] { nameof(Device.Status), nameof(Device.StatusReason), nameof(Device.StatusDescription) });
                await _deviceTaskService.AddSuspendAsync(vaultId);

                transactionScope.Complete();
            }
        }

        public async Task SuspendVaultAsync(string vaultId, string description)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            var vault = await GetVaultByIdAsync(vaultId);
            if (vault == null)
                throw new Exception($"Vault {vaultId} not found");

            if (vault.Status != VaultStatus.Active || vault.Status != VaultStatus.Locked)
                throw new Exception($"Vault {vaultId} status ({vault.Status}) is not allowed to execute this operation");

            vault.Status = VaultStatus.Suspended;
            vault.StatusReason = VaultStatusReason.None;
            vault.StatusDescription = description;

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await GenerateVaultActivationAsync(vaultId);
                await UpdateOnlyPropAsync(vault, new string[] { nameof(Device.Status), nameof(Device.StatusReason), nameof(Device.StatusDescription) });
                await _deviceTaskService.AddSuspendAsync(vaultId);

                transactionScope.Complete();
            }
        }

        public async Task VaultCompromisedAsync(string vaultId)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            var vault = await GetVaultByIdAsync(vaultId);
            if (vault == null)
                throw new Exception($"Vault {vaultId} not found");

            vault.EmployeeId = null;
            vault.Status = VaultStatus.Compromised;

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await UpdateOnlyPropAsync(vault, new string[] { nameof(Device.EmployeeId), nameof(Device.Status) });
                await ChangeVaultActivationStatusAsync(vaultId, HardwareVaultActivationStatus.Canceled);
                await _deviceTaskService.RemoveAllTasksAsync(vaultId);
                await _accountService.RemoveAllAccountsAsync(vaultId);
                await _workstationService.RemoveAllProximityAsync(vaultId);

                transactionScope.Complete();
            }
        }

        public async Task ErrorVaultAsync(string vaultId)
        {
            var vault = await GetVaultByIdAsync(vaultId);

            if (vault == null)
                throw new Exception($"Vault {vaultId} not found");

            if (vault.Status != VaultStatus.Active || vault.Status != VaultStatus.Locked || vault.Status != VaultStatus.Suspended)
                throw new Exception($"Vault {vaultId} status ({vault.Status}) is not allowed to execute this operation");

            vault.Status = VaultStatus.Error;
            await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, new string[] { nameof(Device.Status) });
        }

        #endregion

        #region Profile

        public IQueryable<DeviceAccessProfile> ProfileQuery()
        {
            return _hardwareVaultProfileRepository.Query();
        }

        public async Task<DeviceAccessProfile> GetProfileByIdAsync(string profileId)
        {
            return await _hardwareVaultProfileRepository
                .Query()
                .Include(d => d.Devices)
                .FirstOrDefaultAsync(m => m.Id == profileId);
        }

        public async Task<List<string>> GetVaultIdsByProfileTaskAsync(string profileId)
        {
            return await _deviceTaskService
                .TaskQuery()
                .Where(x => x.Operation == TaskOperation.Profile)
                .Select(x => x.DeviceId)
                .ToListAsync();
        }

        public async Task<List<DeviceAccessProfile>> GetProfilesAsync()
        {
            return await _hardwareVaultProfileRepository
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

            var profile = await _hardwareVaultProfileRepository
                .Query()
                .Where(d => d.Name == deviceAccessProfile.Name)
                .AnyAsync();

            if (profile)
            {
                throw new Exception($"Name {deviceAccessProfile.Name} is already taken.");
            }

            deviceAccessProfile.CreatedAt = DateTime.UtcNow;
            return await _hardwareVaultProfileRepository.AddAsync(deviceAccessProfile);
        }

        public async Task EditProfileAsync(DeviceAccessProfile deviceAccessProfile)
        {
            if (deviceAccessProfile == null)
                throw new ArgumentNullException(nameof(deviceAccessProfile));

            var profile = await _hardwareVaultProfileRepository
               .Query()
               .Where(d => d.Name == deviceAccessProfile.Name && d.Id != deviceAccessProfile.Id)
               .AnyAsync();

            if (profile)
                throw new AlreadyExistException($"Name {deviceAccessProfile.Name} is already taken.");

            deviceAccessProfile.UpdatedAt = DateTime.UtcNow;

            var vaults = await _hardwareVaultRepository
                .Query()
                .Where(x => x.AcceessProfileId == deviceAccessProfile.Id && (x.Status == VaultStatus.Active || x.Status == VaultStatus.Locked || x.Status == VaultStatus.Suspended))
                .ToListAsync();

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultProfileRepository.UpdateAsync(deviceAccessProfile);

                foreach (var vault in vaults)
                {
                    await _deviceTaskService.AddProfileAsync(vault);
                }

                transactionScope.Complete();
            }
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

            var deviceAccessProfile = await _hardwareVaultProfileRepository.GetByIdAsync(id);
            if (deviceAccessProfile == null)
            {
                throw new Exception("Device access profile not found");
            }

            await _hardwareVaultProfileRepository.DeleteAsync(deviceAccessProfile);
        }

        public async Task ChangeVaultProfileAsync(string vaultId, string profileId)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            if (profileId == null)
                throw new ArgumentNullException(nameof(profileId));

            var vault = await GetVaultByIdAsync(vaultId);
            if (vault == null)
                throw new Exception($"Vault {vaultId} not found");

            if (vault.Status != VaultStatus.Active)
                throw new Exception($"Vault {vaultId} status ({vault.Status}) is not allowed to execute this operation");

            var profile = await _hardwareVaultProfileRepository.GetByIdAsync(profileId);
            if (profile == null)
                throw new Exception("Vault profile not found");

            vault.AcceessProfileId = profileId;

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, new string[] { nameof(Device.AcceessProfileId) });
                await _deviceTaskService.AddProfileAsync(vault);

                transactionScope.Complete();
            }
        }

        #endregion
    }
}
