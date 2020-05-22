using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.API.HardwareVault;
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
        private readonly ILicenseService _licenseService;
        private readonly IHardwareVaultTaskService _hardwareVaultTaskService;
        private readonly IAccountService _accountService;
        private readonly IWorkstationService _workstationService;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly IHttpClientFactory _httpClientFactory;

        public HardwareVaultService(IAsyncRepository<HardwareVault> hardwareVaultRepository,
                                    IAsyncRepository<HardwareVaultActivation> hardwareVaultActivationRepository,
                                    IAsyncRepository<HardwareVaultProfile> hardwareVaultProfileRepository,
                                    ILicenseService licenseService,
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
            _licenseService = licenseService;
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
                .Include(d => d.Employee.Department.Company)
                .Include(d => d.HardwareVaultProfile)
                .FirstOrDefaultAsync(m => m.Id == vaultId);
        }

        public async Task<List<HardwareVault>> GetVaultsByEmployeeIdAsync(string employeeId)
        {
            return await _hardwareVaultRepository
                .Query()
                .Include(d => d.Employee.Department.Company)
                .Include(d => d.HardwareVaultProfile)
                .Where(d => d.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<List<HardwareVault>> GetVaultsAsync(int skip, int take, string sortColumn, ListSortDirection sortDirection, string searchText, HardwareVaultFilter filter)
        {
            var query = _hardwareVaultRepository
                .Query()
                .Include(d => d.Employee.Department.Company)
                .Include(d => d.HardwareVaultProfile)
                .AsQueryable();

            // Filter
            if (filter != null)
            {
                if (filter.Id != null)
                {
                    query = query.Where(w => w.Id.Contains(filter.Id, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.MAC != null)
                {
                    query = query.Where(w => w.MAC.Contains(filter.MAC, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Model != null)
                {
                    query = query.Where(w => w.Model.Contains(filter.Model, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.RFID != null)
                {
                    query = query.Where(w => w.RFID.Contains(filter.RFID, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Battery != null)
                {
                    switch (filter.Battery)
                    {
                        case "low":
                            query = query.Where(w => w.Battery <= 30);
                            break;
                        case "high":
                            query = query.Where(w => w.Battery >= 31);
                            break;
                    }
                }
                if (filter.Firmware != null)
                {
                    query = query.Where(w => w.Firmware.Contains(filter.Firmware, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.LastSyncedStartDate != null)
                {
                    query = query.Where(x => x.LastSynced >= filter.LastSyncedStartDate.Value.Date.ToUniversalTime());
                }
                if (filter.LastSyncedEndDate != null)
                {
                    query = query.Where(x => x.LastSynced <= filter.LastSyncedEndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59).ToUniversalTime());
                }
                if (filter.Employee != null)
                {
                    query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(filter.Employee, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Company != null)
                {
                    query = query.Where(x => x.Employee.Department.Company.Name.Contains(filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Department != null)
                {
                    query = query.Where(x => x.Employee.Department.Name.Contains(filter.Department, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.VaultStatus != null)
                {
                    query = query.Where(w => w.Status == filter.VaultStatus);
                }
                if (filter.LicenseStatus != null)
                {
                    query = query.Where(w => w.LicenseStatus == filter.LicenseStatus);
                }
                if (filter.LicenseEndDate != null)
                {
                    query = query.Where(x => x.LicenseEndDate <= filter.LicenseEndDate.Value.Date);
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
                                    (x.Employee.FirstName + " " + x.Employee.LastName).Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Company.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Model.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
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
                case nameof(HardwareVault.HardwareVaultProfile):
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
                case nameof(HardwareVault.Employee):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.FirstName).ThenBy(x => x.Employee.LastName) : query.OrderByDescending(x => x.Employee.FirstName).ThenByDescending(x => x.Employee.LastName);
                    break;
                case nameof(HardwareVault.Employee.Department.Company):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.Department.Company) : query.OrderByDescending(x => x.Employee.Department.Company);
                    break;
                case nameof(HardwareVault.Employee.Department):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.Department.Name) : query.OrderByDescending(x => x.Employee.Department.Name);
                    break;
                case nameof(HardwareVault.Model):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Model) : query.OrderByDescending(x => x.Model);
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
                if (filter.Id != null)
                {
                    query = query.Where(w => w.Id.Contains(filter.Id, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.MAC != null)
                {
                    query = query.Where(w => w.MAC.Contains(filter.MAC, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Model != null)
                {
                    query = query.Where(w => w.Model.Contains(filter.Model, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.RFID != null)
                {
                    query = query.Where(w => w.RFID.Contains(filter.RFID, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Battery != null)
                {
                    switch (filter.Battery)
                    {
                        case "low":
                            query = query.Where(w => w.Battery <= 30);
                            break;
                        case "high":
                            query = query.Where(w => w.Battery >= 31);
                            break;
                    }
                }
                if (filter.Firmware != null)
                {
                    query = query.Where(w => w.Firmware.Contains(filter.Firmware, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.LastSyncedStartDate != null)
                {
                    query = query.Where(x => x.LastSynced >= filter.LastSyncedStartDate.Value.Date.ToUniversalTime());
                }
                if (filter.LastSyncedEndDate != null)
                {
                    query = query.Where(x => x.LastSynced <= filter.LastSyncedEndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59).ToUniversalTime());
                }
                if (filter.Employee != null)
                {
                    query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(filter.Employee, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Company != null)
                {
                    query = query.Where(x => x.Employee.Department.Company.Name.Contains(filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Department != null)
                {
                    query = query.Where(x => x.Employee.Department.Name.Contains(filter.Department, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.VaultStatus != null)
                {
                    query = query.Where(w => w.Status == filter.VaultStatus);
                }
                if (filter.LicenseStatus != null)
                {
                    query = query.Where(w => w.LicenseStatus == filter.LicenseStatus);
                }
                if (filter.LicenseEndDate != null)
                {
                    query = query.Where(x => x.LicenseEndDate <= filter.LicenseEndDate.Value.Date);
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
                                    (x.Employee.FirstName + " " + x.Employee.LastName).Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Company.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Model.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
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

            if (licensing == null)
                throw new Exception("Api Key is empty.");

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(licensing.ApiAddress);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var path = $"api/Devices/GetDevicesWithLicenses/{licensing.ApiKey}";
            var response = await client.GetAsync(path);

            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var importDto = JsonConvert.DeserializeObject<ImportHardwareVaultDto>(data);

                var licenseOrdersToImport = new List<LicenseOrder>();
                var hardwareVaultLicensesToImport = new List<HardwareVaultLicense>();
                var hardwareVaultsToImport = new List<HardwareVault>();

                // Import license orders
                if (importDto.LicenseOrdersDto != null)
                {
                    var licenseOrders = await _licenseService.GetLicenseOrdersAsync();
                    // Remove existing
                    importDto.LicenseOrdersDto.RemoveAll(x => licenseOrders.Select(s => s.Id).Contains(x.Id));

                    foreach (var licenseOrderDto in importDto.LicenseOrdersDto)
                    {
                        licenseOrdersToImport.Add(new LicenseOrder
                        {
                            Id = licenseOrderDto.Id,
                            ContactEmail = licenseOrderDto.ContactEmail,
                            Note = licenseOrderDto.Note,
                            StartDate = licenseOrderDto.StartDate,
                            EndDate = licenseOrderDto.EndDate,
                            ProlongExistingLicenses = licenseOrderDto.ProlongExistingLicenses,
                            CreatedAt = licenseOrderDto.CreatedAt,
                            OrderStatus = licenseOrderDto.OrderStatus
                        });
                    }
                }

                // Import hardware vault licenses
                if (importDto.HardwareVaultLicensesDto != null)
                {
                    var hardwareVaultLicenses = await _licenseService.GetLicensesAsync();
                    // Remove existing
                    importDto.HardwareVaultLicensesDto.RemoveAll(x => hardwareVaultLicenses.Select(s => s.LicenseOrderId).Contains(x.LicenseOrderId) && hardwareVaultLicenses.Select(s => s.HardwareVaultId).Contains(x.HardwareVaultId));

                    foreach (var hardwareVaultLicenseDto in importDto.HardwareVaultLicensesDto)
                    {
                        hardwareVaultLicensesToImport.Add(new HardwareVaultLicense()
                        {
                            HardwareVaultId = hardwareVaultLicenseDto.HardwareVaultId,
                            LicenseOrderId = hardwareVaultLicenseDto.LicenseOrderId,
                            ImportedAt = DateTime.UtcNow,
                            AppliedAt = null,
                            Data = Convert.FromBase64String(hardwareVaultLicenseDto.Data),
                            EndDate = hardwareVaultLicenseDto.EndDate
                        });
                    }
                }

                // Import hardware vaults
                if (importDto.HardwareVaultsDto != null)
                {
                    var vaults = await GetVaultsAsync(0, await GetVaultsCountAsync(string.Empty, null), nameof(HardwareVault.Id), ListSortDirection.Ascending, string.Empty, null);
                    // Remove existing
                    importDto.HardwareVaultsDto.RemoveAll(x => vaults.Select(s => s.Id).Contains(x.HardwareVaultId));

                    foreach (var hardwareVaultDto in importDto.HardwareVaultsDto)
                    {
                        var hardwareVaultLicense = hardwareVaultLicensesToImport.FirstOrDefault(x => x.HardwareVaultId == hardwareVaultDto.HardwareVaultId);

                        hardwareVaultsToImport.Add(new HardwareVault()
                        {
                            Id = hardwareVaultDto.HardwareVaultId,
                            MAC = hardwareVaultDto.MAC,
                            Model = hardwareVaultDto.Model,
                            RFID = hardwareVaultDto.RFID,
                            Battery = 100,
                            Firmware = hardwareVaultDto.Firmware,
                            Status = VaultStatus.Ready,
                            StatusReason = VaultStatusReason.None,
                            StatusDescription = null,
                            LastSynced = null,
                            NeedSync = false,
                            EmployeeId = null,
                            MasterPassword = null,
                            HardwareVaultProfileId = ServerConstants.DefaulHardwareVaultProfileId,
                            ImportedAt = DateTime.UtcNow,
                            HasNewLicense = hardwareVaultLicense == null ? false : true,
                            LicenseStatus = VaultLicenseStatus.None,
                            LicenseEndDate = hardwareVaultLicense == null ? null : hardwareVaultLicense.EndDate
                        });
                    }
                }

                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _hardwareVaultRepository.AddRangeAsync(hardwareVaultsToImport);
                    await _licenseService.AddOrderRangeAsync(licenseOrdersToImport);
                    await _licenseService.AddHardwareVaultLicenseRangeAsync(hardwareVaultLicensesToImport);
                    await _licenseService.UpdateHardwareVaultsLicenseStatusAsync();
                    transactionScope.Complete();
                }
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

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, new string[] { nameof(HardwareVault.Status), nameof(HardwareVault.MasterPassword), nameof(HardwareVault.HasNewLicense) });
                await _licenseService.ChangeLicenseNotAppliedAsync(vaultId);

                transactionScope.Complete();
            }
        }

        public async Task UpdateHardwareVaultInfoAsync(BleDeviceDto dto)
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
            foreach (var vault in vaults)
            {
                vault.NeedSync = needSync;
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

            string employeeId = null;
            if (vault.Employee.HardwareVaults.Count == 1)
                employeeId = vault.EmployeeId;

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
                await _hardwareVaultTaskService.DeleteTasksByVaultIdAsync(vaultId);

                if (employeeId != null)
                    await _accountService.DeleteAccountsByEmployeeIdAsync(employeeId);

                await _workstationService.DeleteProximityByVaultIdAsync(vaultId);

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

        public async Task<HardwareVaultProfile> CreateProfileAsync(HardwareVaultProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            var exist = await _hardwareVaultProfileRepository
                .Query()
                .Where(d => d.Name == profile.Name)
                .AnyAsync();

            if (exist)
            {
                throw new Exception($"Name {profile.Name} is already taken.");
            }

            profile.CreatedAt = DateTime.UtcNow;
            return await _hardwareVaultProfileRepository.AddAsync(profile);
        }

        public async Task EditProfileAsync(HardwareVaultProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            var exist = await _hardwareVaultProfileRepository
               .Query()
               .Where(d => d.Name == profile.Name && d.Id != profile.Id)
               .AnyAsync();

            if (exist)
                throw new AlreadyExistException($"Name {profile.Name} is already taken.");

            profile.UpdatedAt = DateTime.UtcNow;

            var vaults = await _hardwareVaultRepository
                .Query()
                .Where(x => x.HardwareVaultProfileId == profile.Id && (x.Status == VaultStatus.Active || x.Status == VaultStatus.Locked || x.Status == VaultStatus.Suspended))
                .ToListAsync();

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultProfileRepository.UpdateAsync(profile);

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
                throw new Exception("Profile not found");
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
