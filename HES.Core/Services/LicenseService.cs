using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.API.License;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly IAsyncRepository<DeviceLicense> _deviceLicenseRepository;
        private readonly IAsyncRepository<Device> _deviceRepository;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IEmailSenderService _emailSenderService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LicenseService> _logger;

        public LicenseService(IAsyncRepository<LicenseOrder> licenseOrderRepository,
                                   IAsyncRepository<DeviceLicense> deviceLicenseRepository,
                                   IAsyncRepository<Device> deviceRepository,
                                   IAppSettingsService appSettingsService,
                                   IEmailSenderService emailSenderService,
                                   IHttpClientFactory httpClientFactory,
                                   ILogger<LicenseService> logger)
        {
            _licenseOrderRepository = licenseOrderRepository;
            _deviceLicenseRepository = deviceLicenseRepository;
            _deviceRepository = deviceRepository;
            _appSettingsService = appSettingsService;
            _emailSenderService = emailSenderService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        #region HttpClient

        private async Task<HttpClient> CreateHttpClient()
        {
            var licensing = await _appSettingsService.GetLicensingSettingsAsync();

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

        public async Task<LicenseOrder> GetLicenseOrderByIdAsync(string id)
        {
            return await _licenseOrderRepository
                .Query()
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<LicenseOrder> CreateOrderAsync(LicenseOrder licenseOrder)
        {
            if (licenseOrder == null)
            {
                throw new ArgumentNullException(nameof(licenseOrder));
            }

            return await _licenseOrderRepository.AddAsync(licenseOrder);
        }

        public async Task DeleteOrderAsync(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var licenseOrder = await _licenseOrderRepository.GetByIdAsync(id);

            if (licenseOrder == null)
            {
                throw new Exception("Order does not exist.");
            }

            var deviceLicenses = await _deviceLicenseRepository
                .Query()
                .Where(d => d.LicenseOrderId == id)
                .ToListAsync();

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _deviceLicenseRepository.DeleteRangeAsync(deviceLicenses);
                await _licenseOrderRepository.DeleteAsync(licenseOrder);
                transactionScope.Complete();
            }
        }

        // API POST
        public async Task SendOrderAsync(string orderId)
        {
            var order = await GetLicenseOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new Exception("Order not found");
            }

            var deviceLicenses = await GetDeviceLicensesByOrderIdAsync(orderId);
            if (deviceLicenses == null)
            {
                throw new Exception("Device licenses not found");
            }

            var licensing = await _appSettingsService.GetLicensingSettingsAsync();

            var licenseOrderDto = new LicenseOrderDto()
            {
                Id = order.Id,
                ContactEmail = order.ContactEmail,
                CustomerNote = order.Note,
                LicenseStartDate = order.StartDate,
                LicenseEndDate = order.EndDate,
                ProlongExistingLicenses = order.ProlongExistingLicenses,
                CustomerId = licensing.ApiKey,
                Devices = deviceLicenses.Select(d => d.DeviceId).ToList()
            };

            var response = await HttpClientPostOrderAsync(licenseOrderDto);
            if (response.IsSuccessStatusCode)
            {
                order.OrderStatus = OrderStatus.Sent;
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
                if (status == OrderStatus.Undefined)
                {
                    continue;
                }

                // Status has not changed
                if (status == order.OrderStatus)
                {
                    continue;
                }

                if (status == OrderStatus.Completed)
                {
                    await UpdateNewDeviceLicensesAsync(order.Id);
                }

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
                .Where(o => o.OrderStatus == OrderStatus.Sent ||
                            o.OrderStatus == OrderStatus.Processing ||
                            o.OrderStatus == OrderStatus.WaitingForPayment)
                .ToListAsync();
        }

        // API GET
        private async Task<OrderStatus> GetLicenseOrderStatusAsync(string orderId)
        {
            try
            {
                var response = await HttpClientGetLicenseOrderStatusAsync(orderId);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<OrderStatus>(data);
                }
                _logger.LogCritical($"{response.StatusCode.ToString()} {response.Content.ReadAsStringAsync()}");
                return OrderStatus.Error;
            }
            catch (Exception)
            {
                return OrderStatus.Undefined;
            }
        }

        #endregion

        #region License

        public async Task<IList<DeviceLicense>> GetDeviceLicensesByDeviceIdAsync(string deviceId)
        {
            return await _deviceLicenseRepository
                .Query()
                .Where(d => d.AppliedAt == null && d.DeviceId == deviceId)
                .ToListAsync();
        }

        public async Task<IList<DeviceLicense>> GetDeviceLicensesByOrderIdAsync(string orderId)
        {
            return await _deviceLicenseRepository
                .Query()
                .Where(d => d.LicenseOrderId == orderId)
                .ToListAsync();
        }

        public async Task<List<DeviceLicense>> AddDeviceLicensesAsync(string orderId, List<string> devicesIds)
        {
            if (devicesIds == null)
            {
                throw new ArgumentNullException(nameof(devicesIds));
            }

            var deviceLicenses = new List<DeviceLicense>();

            foreach (var deviceId in devicesIds)
            {
                deviceLicenses.Add(new DeviceLicense()
                {
                    LicenseOrderId = orderId,
                    DeviceId = deviceId
                });
            }

            return await _deviceLicenseRepository.AddRangeAsync(deviceLicenses) as List<DeviceLicense>;
        }

        public async Task UpdateDeviceLicenseStatusAsync()
        {
            var devicesChangedStatus = new List<Device>();

            var devices = await _deviceRepository
                .Query()
                .Where(d => d.LicenseEndDate != null)
                .ToListAsync();

            foreach (var device in devices)
            {
                if (device.LicenseEndDate.Value.Date.Subtract(DateTime.UtcNow.Date).TotalDays > 90)
                {
                    if (device.LicenseStatus != LicenseStatus.Valid)
                    {
                        device.LicenseStatus = LicenseStatus.Valid;
                        devicesChangedStatus.Add(device);
                    }
                }
                else if (device.LicenseEndDate.Value.Date.Subtract(DateTime.UtcNow.Date).TotalDays > 30)
                {
                    if (device.LicenseStatus != LicenseStatus.Warning)
                    {
                        device.LicenseStatus = LicenseStatus.Warning;
                        devicesChangedStatus.Add(device);
                    }
                }
                else if (device.LicenseEndDate.Value.Date.Subtract(DateTime.UtcNow.Date).TotalDays > 0)
                {
                    if (device.LicenseStatus != LicenseStatus.Critical)
                    {
                        device.LicenseStatus = LicenseStatus.Critical;
                        devicesChangedStatus.Add(device);
                    }
                }
                else if (device.LicenseEndDate.Value.Date.Subtract(DateTime.UtcNow.Date).TotalDays < 0)
                {
                    if (device.LicenseStatus != LicenseStatus.Expired)
                    {
                        device.LicenseStatus = LicenseStatus.Expired;
                        devicesChangedStatus.Add(device);
                    }
                }
            }

            if (devicesChangedStatus.Count > 0)
            {
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _deviceRepository.UpdateOnlyPropAsync(devices, new string[] { "LicenseStatus" });
                    await _emailSenderService.SendDeviceLicenseStatus(devicesChangedStatus);
                    transactionScope.Complete();
                }
            }
        }

        public async Task SetDeviceLicenseAppliedAsync(string deviceId, string licenseId)
        {
            var deviceLicense = await _deviceLicenseRepository
                .Query()
                .Where(d => d.DeviceId == deviceId && d.Id == licenseId)
                .FirstOrDefaultAsync();

            if (deviceLicense == null)
            {
                throw new Exception("Device license not found.");
            }

            deviceLicense.AppliedAt = DateTime.UtcNow;
            await _deviceLicenseRepository.UpdateOnlyPropAsync(deviceLicense, new string[] { "AppliedAt" });

            var existLicenses = await GetDeviceLicensesByDeviceIdAsync(deviceId);
            if (existLicenses.Count == 0)
            {
                var device = await _deviceRepository.GetByIdAsync(deviceId);
                if (device == null)
                {
                    throw new Exception("Device not found.");
                }
                device.HasNewLicense = false;
                device.LicenseEndDate = deviceLicense.EndDate;
                await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "HasNewLicense", "LicenseEndDate" });
            }
        }

        public async Task DiscardLicenseAppliedAsync(string deviceId)
        {
            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device != null)
            {
                device.HasNewLicense = true;
                await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "HasNewLicense" });
            }

            var licenses = await _deviceLicenseRepository
                .Query()
                .Where(d => d.DeviceId == deviceId)
                .ToListAsync();

            foreach (var license in licenses)
            {
                license.AppliedAt = null;
            }
            await _deviceLicenseRepository.UpdateOnlyPropAsync(licenses, new string[] { "AppliedAt" });
        }

        // API GET
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
                var currentLicenses = await GetDeviceLicensesByOrderIdAsync(orderId);
                // Get devices to update
                var devicesIds = newLicenses.Select(d => d.DeviceId).ToList();
                var devices = await _deviceRepository.Query().Where(x => devicesIds.Contains(x.Id)).ToListAsync();

                foreach (var newLicense in newLicenses)
                {
                    var currentLicense = currentLicenses.FirstOrDefault(c => c.DeviceId == newLicense.DeviceId);
                    currentLicense.ImportedAt = DateTime.UtcNow;
                    currentLicense.EndDate = newLicense.LicenseEndDate;
                    currentLicense.Data = Convert.FromBase64String(newLicense.Data);

                    var device = devices.FirstOrDefault(d => d.Id == newLicense.DeviceId);
                    device.HasNewLicense = true;
                    device.LicenseEndDate = currentLicense.EndDate;
                }
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _deviceLicenseRepository.UpdateOnlyPropAsync(currentLicenses, new string[] { "ImportedAt", "EndDate", "Data" });
                    await _deviceRepository.UpdateOnlyPropAsync(devices, new string[] { "HasNewLicense", "LicenseEndDate" });
                    transactionScope.Complete();
                }
            }
        }

        #endregion
    }
}
