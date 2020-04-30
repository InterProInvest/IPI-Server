using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.API.Device;
using HES.Core.Models.Web.HardwareVault;
using Hideez.SDK.Communication.HES.DTO;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Core.Services
{
    public class HardwareVaultService : IHardwareVaultService
    {
        private readonly IAsyncRepository<HardwareVault> _hardwareVaultRepository;
        private readonly IAsyncRepository<HardwareVaultActivation> _hardwareVaultActivationRepository;
        private readonly IAsyncRepository<HardwareVaultProfile> _hardwareVaultProfileRepository;
        private readonly IAsyncRepository<HardwareVaultLicense> _hardwareVaultLicenseRepository;
        private readonly IHardwareVaultTaskService _hardwareVaultTaskService;
        private readonly IAccountService _accountService;
        private readonly IWorkstationService _workstationService;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly IHttpClientFactory _httpClientFactory;

        public HardwareVaultService(IAsyncRepository<HardwareVault> hardwareVaultRepository,
                                     IAsyncRepository<HardwareVaultActivation> hardwareVaultActivationRepository,
                                     IAsyncRepository<HardwareVaultProfile> hardwareVaultProfileRepository,
                                     IAsyncRepository<HardwareVaultLicense> hardwareVaultLicenseRepository,
                                     IHardwareVaultTaskService hardwareVaultTaskService,
                                     IAccountService accountService,
                                     IWorkstationService workstationService,
                                     IAppSettingsService appSettingsService,
                                     IDataProtectionService dataProtectionService,
                                     IHttpClientFactory httpClientFactory)
        {
            _hardwareVaultRepository = hardwareVaultRepository;
            _hardwareVaultActivationRepository = hardwareVaultActivationRepository;
            _hardwareVaultProfileRepository = hardwareVaultProfileRepository;
            _hardwareVaultLicenseRepository = hardwareVaultLicenseRepository;
            _hardwareVaultTaskService = hardwareVaultTaskService;
            _accountService = accountService;
            _workstationService = workstationService;
            _appSettingsService = appSettingsService;
            _dataProtectionService = dataProtectionService;
            _httpClientFactory = httpClientFactory;
        }

        #region Vault

        public IQueryable<HardwareVault> VaultQuery()
        {
            return _hardwareVaultRepository.Query();
        }

        public async Task<HardwareVault> GetVaultByIdAsync(string vaultId)
        {
            return await _hardwareVaultRepository
                .Query()
                .Include(d => d.Employee)
                .Include(d => d.HardwareVaultProfile)
                .FirstOrDefaultAsync(m => m.Id == vaultId);
        }

        public async Task<List<HardwareVault>> GetVaultsByEmployeeIdAsync(string employeeId)
        {
            return await _hardwareVaultRepository
                .Query()
                .Include(d => d.Employee)
                .Include(d => d.HardwareVaultProfile)
                .Where(d => d.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<List<HardwareVault>> GetVaultsAsync(int skip, int take, string sortColumn, ListSortDirection sortDirection, string searchText, HardwareVaultFilter filter)
        {
            var query = _hardwareVaultRepository
                .Query()
                .Include(d => d.HardwareVaultProfile)
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
                                    x.HardwareVaultProfile.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
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
                case nameof(HardwareVault.Id):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id);
                    break;
                case nameof(HardwareVault.MAC):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.MAC) : query.OrderByDescending(x => x.MAC);
                    break;
                case nameof(HardwareVault.Battery):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Battery) : query.OrderByDescending(x => x.Battery);
                    break;
                case nameof(HardwareVault.Firmware):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Firmware) : query.OrderByDescending(x => x.Firmware);
                    break;
                case nameof(HardwareVault.HardwareVaultProfile.Name):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.HardwareVaultProfile.Name) : query.OrderByDescending(x => x.HardwareVaultProfile.Name);
                    break;
                case nameof(HardwareVault.Status):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status);
                    break;
                case nameof(HardwareVault.LastSynced):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.LastSynced) : query.OrderByDescending(x => x.LastSynced);
                    break;
                case nameof(HardwareVault.LicenseStatus):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.LicenseStatus) : query.OrderByDescending(x => x.LicenseStatus);
                    break;
                case nameof(HardwareVault.LicenseEndDate):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.LicenseEndDate) : query.OrderByDescending(x => x.LicenseEndDate);
                    break;
                case nameof(HardwareVault.Employee.FullName):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.FirstName).ThenBy(x => x.Employee.LastName) : query.OrderByDescending(x => x.Employee.FirstName).ThenByDescending(x => x.Employee.LastName);
                    break;
                case nameof(HardwareVault.Employee.EmpCompany):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.Department.Company) : query.OrderByDescending(x => x.Employee.Department.Company);
                    break;
                case nameof(HardwareVault.Model):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Model) : query.OrderByDescending(x => x.Model);
                    break;
                case nameof(HardwareVault.Employee.Department):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.Department.Name) : query.OrderByDescending(x => x.Employee.Department.Name);
                    break;
                case nameof(HardwareVault.RFID):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.RFID) : query.OrderByDescending(x => x.RFID);
                    break;
            }

            return await query.Skip(skip).Take(take).ToListAsync();
        }

        public async Task<int> GetVaultsCountAsync(string searchText, HardwareVaultFilter filter)
        {
            var query = _hardwareVaultRepository
                .Query()
                .Include(d => d.HardwareVaultProfile)
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
                                    x.HardwareVaultProfile.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
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

        public async Task<HardwareVault> AddVaultIfNotExistAsync(HardwareVault vault)
        {
            if (vault == null)
                throw new ArgumentNullException(nameof(vault));

            return await _hardwareVaultRepository.AddAsync(vault);
        }

        public async Task ImportVaultsAsync()
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
                var devices = await GetVaultsAsync(0, devicesCount, nameof(HardwareVault.Id), ListSortDirection.Ascending, string.Empty, null);
                var devicesToImport = new List<HardwareVault>();
                devicesDto.RemoveAll(x => devices.Select(s => s.Id).Contains(x.DeviceId));

                foreach (var deviceDto in devicesDto)
                {
                    devicesToImport.Add(new HardwareVault()
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
                        HardwareVaultProfileId = ServerConstants.DefaulAccessProfileId,
                        ImportedAt = DateTime.UtcNow,
                        HasNewLicense = false,
                        LicenseStatus = VaultLicenseStatus.None,
                        LicenseEndDate = null
                    });
                }

                await _hardwareVaultRepository.AddRangeAsync(devicesToImport);
            }
        }

        public async Task EditRfidAsync(HardwareVault vault)
        {
            if (vault == null)
            {
                throw new ArgumentNullException(nameof(vault));
            }

            await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, new string[] { "RFID" });
        }

        public Task UnchangedVaultAsync(HardwareVault vault)
        {
            return _hardwareVaultRepository.Unchanged(vault);
        }

        public async Task UpdateOnlyPropAsync(HardwareVault vault, string[] properties)
        {
            await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, properties);
        }

        public async Task UpdateAfterWipe(string vaultId)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            var vault = await GetVaultByIdAsync(vaultId);

            if (vault == null)
                throw new Exception($"Vault {vault.Id} not found");

            vault.Status = VaultStatus.Ready;
            vault.MasterPassword = null;
            vault.HasNewLicense = true;

            var licenses = await _hardwareVaultLicenseRepository
                .Query()
                .Where(d => d.HardwareVaultId == vaultId)
                .ToListAsync();

            licenses.ForEach(x => x.AppliedAt = null);

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, new string[] { nameof(HardwareVault.Status), nameof(HardwareVault.MasterPassword), nameof(HardwareVault.HasNewLicense) });
                await _hardwareVaultLicenseRepository.UpdateOnlyPropAsync(licenses, new string[] { nameof(HardwareVaultLicense.AppliedAt) });

                transactionScope.Complete();
            }
        }

        public async Task UpdateDeviceInfoAsync(BleDeviceDto dto)
        {
            var vault = await _hardwareVaultRepository.GetByIdAsync(dto.DeviceSerialNo);
            if (vault == null)
                throw new Exception($"Vault {dto.DeviceSerialNo} not found");

            if (dto.IsLocked)
            {
                if (!dto.IsCanUnlock && (vault.Status == VaultStatus.Active || vault.Status == VaultStatus.Reserved || vault.Status == VaultStatus.Suspended))
                {
                    vault.Status = VaultStatus.Locked;
                }
            }
            else
            {
                if (vault.Status == VaultStatus.Suspended || vault.Status == VaultStatus.Reserved)
                {
                    vault.Status = VaultStatus.Active;
                }
            }

            vault.Battery = dto.Battery;
            vault.Firmware = dto.FirmwareVersion;
            vault.LastSynced = DateTime.UtcNow;

            await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, new string[] { nameof(HardwareVault.Battery), nameof(HardwareVault.Firmware), nameof(HardwareVault.Status), nameof(HardwareVault.LastSynced) });
        }

        public async Task UpdateNeedSyncAsync(HardwareVault vault, bool needSync)
        {
            vault.NeedSync = needSync;
            await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, new string[] { nameof(HardwareVault.NeedSync) });
        }

        public async Task UpdateNeedSyncAsync(IList<HardwareVault> vaults, bool needSync)
        {
            foreach (var device in vaults)
            {
                device.NeedSync = needSync;
            }
            await _hardwareVaultRepository.UpdateOnlyPropAsync(vaults, new string[] { nameof(HardwareVault.NeedSync) });
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
            vault.StatusDescription = null;

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await GenerateVaultActivationAsync(vaultId);
                await UpdateOnlyPropAsync(vault, new string[] { nameof(HardwareVault.Status), nameof(HardwareVault.StatusReason), nameof(HardwareVault.StatusDescription) });
                await _hardwareVaultTaskService.AddSuspendAsync(vaultId);

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

            if (vault.Status != VaultStatus.Active)
                throw new Exception($"Vault {vaultId} status ({vault.Status}) is not allowed to execute this operation");

            vault.Status = VaultStatus.Suspended;
            vault.StatusReason = VaultStatusReason.None;
            vault.StatusDescription = description;

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await GenerateVaultActivationAsync(vaultId);
                await UpdateOnlyPropAsync(vault, new string[] { nameof(HardwareVault.Status), nameof(HardwareVault.StatusReason), nameof(HardwareVault.StatusDescription) });
                await _hardwareVaultTaskService.AddSuspendAsync(vaultId);

                transactionScope.Complete();
            }
        }

        public async Task VaultCompromisedAsync(string vaultId, VaultStatusReason reason, string description)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            var vault = await GetVaultByIdAsync(vaultId);
            if (vault == null)
                throw new Exception($"Vault {vaultId} not found");

            vault.EmployeeId = null;
            vault.MasterPassword = null;
            vault.NeedSync = false;
            vault.Status = VaultStatus.Compromised;
            vault.StatusReason = reason;
            vault.StatusDescription = description;

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await UpdateOnlyPropAsync(vault, new string[] { nameof(HardwareVault.EmployeeId), nameof(HardwareVault.MasterPassword), nameof(HardwareVault.NeedSync), nameof(HardwareVault.Status), nameof(HardwareVault.StatusReason), nameof(HardwareVault.StatusDescription) });
                await ChangeVaultActivationStatusAsync(vaultId, HardwareVaultActivationStatus.Canceled);
                await _hardwareVaultTaskService.RemoveAllTasksAsync(vaultId);
                await _accountService.RemoveAllAccountsAsync(vaultId);
                await _workstationService.RemoveAllProximityAsync(vaultId);

                transactionScope.Complete();
            }
        }

        #endregion

        #region Vault Profile

        public IQueryable<HardwareVaultProfile> ProfileQuery()
        {
            return _hardwareVaultProfileRepository.Query();
        }

        public async Task<HardwareVaultProfile> GetProfileByIdAsync(string profileId)
        {
            return await _hardwareVaultProfileRepository
                .Query()
                .Include(d => d.HardwareVaults)
                .FirstOrDefaultAsync(m => m.Id == profileId);
        }

        public async Task<List<string>> GetVaultIdsByProfileTaskAsync(string profileId)
        {
            return await _hardwareVaultTaskService
                .TaskQuery()
                .Where(x => x.Operation == TaskOperation.Profile)
                .Select(x => x.HardwareVaultId)
                .ToListAsync();
        }

        public async Task<List<HardwareVaultProfile>> GetProfilesAsync()
        {
            return await _hardwareVaultProfileRepository
                .Query()
                .Include(d => d.HardwareVaults)
                .ToListAsync();
        }

        public async Task<HardwareVaultProfile> CreateProfileAsync(HardwareVaultProfile deviceAccessProfile)
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

        public async Task EditProfileAsync(HardwareVaultProfile deviceAccessProfile)
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
                .Where(x => x.HardwareVaultProfileId == deviceAccessProfile.Id && (x.Status == VaultStatus.Active || x.Status == VaultStatus.Locked || x.Status == VaultStatus.Suspended))
                .ToListAsync();

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultProfileRepository.UpdateAsync(deviceAccessProfile);

                foreach (var vault in vaults)
                {
                    await _hardwareVaultTaskService.AddProfileAsync(vault);
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

            vault.HardwareVaultProfileId = profileId;

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, new string[] { nameof(HardwareVault.HardwareVaultProfileId) });
                await _hardwareVaultTaskService.AddProfileAsync(vault);

                transactionScope.Complete();
            }
        }

        #endregion
    }
}
