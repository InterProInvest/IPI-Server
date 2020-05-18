using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.API.License;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Core.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly IAsyncRepository<LicenseOrder> _licenseOrderRepository;
        private readonly IAsyncRepository<HardwareVaultLicense> _hardwareVaultLicenseRepository;
        private readonly IAsyncRepository<HardwareVault> _hardwareVaultRepository;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IEmailSenderService _emailSenderService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LicenseService> _logger;

        public LicenseService(IAsyncRepository<LicenseOrder> licenseOrderRepository,
                                   IAsyncRepository<HardwareVaultLicense> hardwareVaultLicenseRepository,
                                   IAsyncRepository<HardwareVault> hardwareVaultRepository,
                                   IAppSettingsService appSettingsService,
                                   IEmailSenderService emailSenderService,
                                   IHttpClientFactory httpClientFactory,
                                   ILogger<LicenseService> logger)
        {
            _licenseOrderRepository = licenseOrderRepository;
            _hardwareVaultLicenseRepository = hardwareVaultLicenseRepository;
            _hardwareVaultRepository = hardwareVaultRepository;
            _appSettingsService = appSettingsService;
            _emailSenderService = emailSenderService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        #region HttpClient

        private async Task<HttpClient> CreateHttpClient()
        {
            var licensing = await _appSettingsService.GetLicensingSettingsAsync();

            if (licensing == null)
                throw new Exception("Api Key is empty.");

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(licensing.ApiAddress);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        private async Task<HttpResponseMessage> HttpClientPostOrderAsync(LicenseOrderDto licenseOrderDto)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(licenseOrderDto), Encoding.UTF8, "application/json");
            var path = $"api/Licenses/CreateLicenseOrder";
            var client = await CreateHttpClient();
            return await client.PostAsync(path, stringContent);
        }

        private async Task<HttpResponseMessage> HttpClientGetLicenseOrderStatusAsync(string orderId)
        {
            var path = $"api/Licenses/GetLicenseOrderStatus/{orderId}";
            var client = await CreateHttpClient();
            return await client.GetAsync(path);
        }

        private async Task<HttpResponseMessage> HttpClientGetDeviceLicensesAsync(string orderId)
        {
            var path = $"/api/Licenses/GetDeviceLicenses/{orderId}";
            var client = await CreateHttpClient();
            return await client.GetAsync(path);
        }

        #endregion

        #region Order

        public async Task<List<LicenseOrder>> GetLicenseOrdersAsync()
        {
            return await _licenseOrderRepository.Query().ToListAsync();
        }

        public async Task<LicenseOrder> GetLicenseOrderByIdAsync(string orderId)
        {
            return await _licenseOrderRepository
                .Query()
                .FirstOrDefaultAsync(x => x.Id == orderId);
        }

        public async Task<LicenseOrder> CreateOrderAsync(LicenseOrder licenseOrder)
        {
            if (licenseOrder == null)
                throw new ArgumentNullException(nameof(licenseOrder));

            return await _licenseOrderRepository.AddAsync(licenseOrder);
        }

        public async Task DeleteOrderAsync(string orderId)
        {
            if (orderId == null)
                throw new ArgumentNullException(nameof(orderId));

            var licenseOrder = await _licenseOrderRepository.GetByIdAsync(orderId);

            if (licenseOrder == null)
                throw new Exception("Order does not exist.");

            var deviceLicenses = await _hardwareVaultLicenseRepository
                .Query()
                .Where(d => d.LicenseOrderId == orderId)
                .ToListAsync();

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultLicenseRepository.DeleteRangeAsync(deviceLicenses);
                await _licenseOrderRepository.DeleteAsync(licenseOrder);
                transactionScope.Complete();
            }
        }

        public async Task SendOrderAsync(string orderId)
        {
            var order = await GetLicenseOrderByIdAsync(orderId);
            if (order == null)
                throw new Exception("Order not found.");

            var vaultLicenses = await GetLicensesByOrderIdAsync(orderId);
            if (vaultLicenses == null)
                throw new Exception("Hardware vault licenses not found.");

            var licensing = await _appSettingsService.GetLicensingSettingsAsync();

            if (licensing == null)
                throw new Exception("Api Key is empty.");

            var licenseOrderDto = new LicenseOrderDto()
            {
                Id = order.Id,
                ContactEmail = order.ContactEmail,
                CustomerNote = order.Note,
                LicenseStartDate = order.StartDate,
                LicenseEndDate = order.EndDate,
                ProlongExistingLicenses = order.ProlongExistingLicenses,
                CustomerId = licensing.ApiKey,
                Devices = vaultLicenses.Select(d => d.HardwareVaultId).ToList()
            };

            var response = await HttpClientPostOrderAsync(licenseOrderDto);
            if (response.IsSuccessStatusCode)
            {
                order.OrderStatus = LicenseOrderStatus.Sent;
                await _licenseOrderRepository.UpdateOnlyPropAsync(order, new string[] { "OrderStatus" });
                return;
            }

            var ex = await response.Content.ReadAsStringAsync();
            throw new Exception(ex);
        }

        public async Task UpdateLicenseOrdersAsync()
        {
            var orders = await GetOpenLicenseOrdersAsync();

            foreach (var order in orders)
            {
                var status = await GetLicenseOrderStatusAsync(order.Id);

                // Http transport error
                if (status == LicenseOrderStatus.Undefined)
                    continue;

                // Status has not changed
                if (status == order.OrderStatus)
                    continue;

                if (status == LicenseOrderStatus.Completed)
                    await UpdateNewDeviceLicensesAsync(order.Id);

                order.OrderStatus = status;

                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _licenseOrderRepository.UpdateOnlyPropAsync(order, new string[] { "OrderStatus" });
                    await _emailSenderService.SendLicenseChangedAsync(order.CreatedAt, status);
                    transactionScope.Complete();
                }
            }
        }

        private async Task<List<LicenseOrder>> GetOpenLicenseOrdersAsync()
        {
            return await _licenseOrderRepository
                .Query()
                .Where(x => x.OrderStatus == LicenseOrderStatus.Sent ||
                            x.OrderStatus == LicenseOrderStatus.Processing ||
                            x.OrderStatus == LicenseOrderStatus.WaitingForPayment)
                .ToListAsync();
        }

        private async Task<LicenseOrderStatus> GetLicenseOrderStatusAsync(string orderId)
        {
            try
            {
                var response = await HttpClientGetLicenseOrderStatusAsync(orderId);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<LicenseOrderStatus>(data);
                }

                _logger.LogCritical($"{response.StatusCode.ToString()} {response.Content.ReadAsStringAsync()}");
                return LicenseOrderStatus.Error;
            }
            catch (Exception)
            {
                return LicenseOrderStatus.Undefined;
            }
        }

        #endregion

        #region License

        public async Task<IList<HardwareVaultLicense>> GetNotAppliedLicensesByHardwareVaultIdAsync(string vaultId)
        {
            return await _hardwareVaultLicenseRepository
                .Query()
                .Where(d => d.AppliedAt == null && d.HardwareVaultId == vaultId && d.Data != null)
                .ToListAsync();
        }

        public async Task<IList<HardwareVaultLicense>> GetLicensesByOrderIdAsync(string orderId)
        {
            return await _hardwareVaultLicenseRepository
                .Query()
                .Where(d => d.LicenseOrderId == orderId)
                .ToListAsync();
        }

        public async Task<List<HardwareVaultLicense>> AddHardwareVaultLicensesAsync(string orderId, List<string> vaultIds)
        {
            if (vaultIds == null)
                throw new ArgumentNullException(nameof(vaultIds));

            var hardwareVaultLicenses = new List<HardwareVaultLicense>();

            foreach (var vaultId in vaultIds)
            {
                hardwareVaultLicenses.Add(new HardwareVaultLicense()
                {
                    LicenseOrderId = orderId,
                    HardwareVaultId = vaultId
                });
            }

            return await _hardwareVaultLicenseRepository.AddRangeAsync(hardwareVaultLicenses) as List<HardwareVaultLicense>;
        }

        public async Task UpdatehardwareVaultsLicenseStatusAsync()
        {
            var hardwareVaultsChangedStatus = new List<HardwareVault>();

            var hardwareVaults = await _hardwareVaultRepository
                .Query()
                .Where(d => d.LicenseEndDate != null)
                .ToListAsync();

            foreach (var hardwareVault in hardwareVaults)
            {
                if (hardwareVault.LicenseEndDate.Value.Date.Subtract(DateTime.UtcNow.Date).TotalDays > 90)
                {
                    if (hardwareVault.LicenseStatus != VaultLicenseStatus.Valid)
                    {
                        hardwareVault.LicenseStatus = VaultLicenseStatus.Valid;
                        hardwareVaultsChangedStatus.Add(hardwareVault);
                    }
                }
                else if (hardwareVault.LicenseEndDate.Value.Date.Subtract(DateTime.UtcNow.Date).TotalDays > 30)
                {
                    if (hardwareVault.LicenseStatus != VaultLicenseStatus.Warning)
                    {
                        hardwareVault.LicenseStatus = VaultLicenseStatus.Warning;
                        hardwareVaultsChangedStatus.Add(hardwareVault);
                    }
                }
                else if (hardwareVault.LicenseEndDate.Value.Date.Subtract(DateTime.UtcNow.Date).TotalDays > 0)
                {
                    if (hardwareVault.LicenseStatus != VaultLicenseStatus.Critical)
                    {
                        hardwareVault.LicenseStatus = VaultLicenseStatus.Critical;
                        hardwareVaultsChangedStatus.Add(hardwareVault);
                    }
                }
                else if (hardwareVault.LicenseEndDate.Value.Date.Subtract(DateTime.UtcNow.Date).TotalDays < 0)
                {
                    if (hardwareVault.LicenseStatus != VaultLicenseStatus.Expired)
                    {
                        hardwareVault.LicenseStatus = VaultLicenseStatus.Expired;
                        hardwareVaultsChangedStatus.Add(hardwareVault);
                    }
                }
            }

            if (hardwareVaultsChangedStatus.Count > 0)
            {
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _hardwareVaultRepository.UpdateOnlyPropAsync(hardwareVaults, new string[] { nameof(HardwareVault.LicenseStatus) });
                    await _emailSenderService.SendHardwareVaultLicenseStatus(hardwareVaultsChangedStatus);
                    transactionScope.Complete();
                }
            }
        }

        public async Task ChangeLicenseAppliedAsync(string vaultId, string licenseId)
        {
            var hardwareVaultLicense = await _hardwareVaultLicenseRepository
                .Query()
                .Where(d => d.HardwareVaultId == vaultId && d.Id == licenseId)
                .FirstOrDefaultAsync();

            if (hardwareVaultLicense == null)
                throw new Exception("Hardware vault license not found.");

            hardwareVaultLicense.AppliedAt = DateTime.UtcNow;
            await _hardwareVaultLicenseRepository.UpdateOnlyPropAsync(hardwareVaultLicense, new string[] { nameof(HardwareVaultLicense.AppliedAt) });

            var notAppliedLicenses = await GetNotAppliedLicensesByHardwareVaultIdAsync(vaultId);
            if (notAppliedLicenses.Count == 0)
            {
                var vault = await _hardwareVaultRepository.GetByIdAsync(vaultId);

                if (vault == null)
                    throw new Exception("Device not found.");

                vault.HasNewLicense = false;
                vault.LicenseEndDate = hardwareVaultLicense.EndDate;
                await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, new string[] { nameof(HardwareVault.HasNewLicense), nameof(HardwareVault.LicenseEndDate) });
            }
        }

        public async Task ChangeLicenseNotAppliedAsync(string vaultId)
        {
            var vault = await _hardwareVaultRepository.GetByIdAsync(vaultId);
            if (vault != null)
            {
                vault.HasNewLicense = true;
                await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, new string[] { nameof(HardwareVault.HasNewLicense) });
            }

            var licenses = await _hardwareVaultLicenseRepository
                .Query()
                .Where(d => d.HardwareVaultId == vaultId)
                .ToListAsync();

            foreach (var license in licenses)
            {
                license.AppliedAt = null;
            }
            await _hardwareVaultLicenseRepository.UpdateOnlyPropAsync(licenses, new string[] { nameof(HardwareVaultLicense.AppliedAt) });
        }

        private async Task UpdateNewDeviceLicensesAsync(string orderId)
        {
            if (orderId == null)
            {
                throw new ArgumentNullException(nameof(orderId));
            }

            var response = await HttpClientGetDeviceLicensesAsync(orderId);

            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                // Deserialize new licenses
                var data = await response.Content.ReadAsStringAsync();
                var newLicenses = JsonConvert.DeserializeObject<List<DeviceLicenseDto>>(data);
                // Get current licenses to update
                var currentLicenses = await GetLicensesByOrderIdAsync(orderId);
                // Get devices to update
                var devicesIds = newLicenses.Select(d => d.DeviceId).ToList();
                var devices = await _hardwareVaultRepository.Query().Where(x => devicesIds.Contains(x.Id)).ToListAsync();

                foreach (var newLicense in newLicenses)
                {
                    var currentLicense = currentLicenses.FirstOrDefault(c => c.HardwareVaultId == newLicense.DeviceId);
                    currentLicense.ImportedAt = DateTime.UtcNow;
                    currentLicense.EndDate = newLicense.LicenseEndDate;
                    currentLicense.Data = Convert.FromBase64String(newLicense.Data);

                    var device = devices.FirstOrDefault(d => d.Id == newLicense.DeviceId);
                    device.HasNewLicense = true;
                    device.LicenseEndDate = currentLicense.EndDate;
                }
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _hardwareVaultLicenseRepository.UpdateOnlyPropAsync(currentLicenses, new string[] { "ImportedAt", "EndDate", "Data" });
                    await _hardwareVaultRepository.UpdateOnlyPropAsync(devices, new string[] { "HasNewLicense", "LicenseEndDate" });
                    transactionScope.Complete();
                }
            }
        }

        #endregion
    }
}
