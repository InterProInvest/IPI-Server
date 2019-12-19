﻿using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.API.License;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class LicenseService : ILicenseService
    {
        private const string ApiBaseAddress = "http://192.168.10.249:8070/";
        //private const string ApiBaseAddress = "https://localhost:44388/";
        private readonly IAsyncRepository<LicenseOrder> _licenseOrderRepository;
        private readonly IAsyncRepository<DeviceLicense> _deviceLicenseRepository;
        private readonly IAsyncRepository<Device> _deviceRepository;

        public LicenseService(IAsyncRepository<LicenseOrder> licenseOrderRepository,
                                   IAsyncRepository<DeviceLicense> deviceLicenseRepository,
                                   IAsyncRepository<Device> deviceRepository)
        {
            _licenseOrderRepository = licenseOrderRepository;
            _deviceLicenseRepository = deviceLicenseRepository;
            _deviceRepository = deviceRepository;
        }

        #region Order

        public IQueryable<LicenseOrder> LicenseOrderQuery()
        {
            return _licenseOrderRepository.Query();
        }

        public async Task<LicenseOrder> GetLicenseOrderByIdAsync(string id)
        {
            return await _licenseOrderRepository
                .Query()
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<LicenseOrder>> GetLicenseOrdersAsync()
        {
            return await _licenseOrderRepository.Query().ToListAsync();
        }

        public async Task<List<LicenseOrder>> GetOpenLicenseOrdersAsync()
        {
            return await _licenseOrderRepository
                .Query()
                .Where(o => o.OrderStatus == OrderStatus.Sent ||
                            o.OrderStatus == OrderStatus.Processing ||
                            o.OrderStatus == OrderStatus.WaitingForPayment)
                .ToListAsync();
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
            await _licenseOrderRepository.DeleteAsync(licenseOrder);
        }

        public async Task SendOrderAsync(string orderId)
        {
            var order = await GetLicenseOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new Exception("Order not found");
            }

            var devices = await GetDeviceLicensesByOrderIdAsync(orderId);
            if (devices == null)
            {
                throw new Exception("Device licenses not found");
            }

            var licenseOrderDto = new LicenseOrderDto()
            {
                Id = order.Id,
                ContactEmail = order.ContactEmail,
                CustomerNote = order.Note,
                LicenseStartDate = order.StartDate,
                LicenseEndDate = order.EndDate,
                ProlongExistingLicenses = order.ProlongExistingLicenses,
                CustomerId = "BBB26599-81B8-44D5-80C0-31CF830F1578",
                Devices = devices.Select(d => d.DeviceId).ToList()
            };

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(ApiBaseAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var stringContent = new StringContent(JsonConvert.SerializeObject(licenseOrderDto), Encoding.UTF8, "application/json");
                var path = $"api/Licenses/CreateLicenseOrder";

                HttpResponseMessage response = await client.PostAsync(path, stringContent);
                if (response.IsSuccessStatusCode)
                {
                    order.OrderStatus = OrderStatus.Sent;
                    await _licenseOrderRepository.UpdateOnlyPropAsync(order, new string[] { "OrderStatus" });
                    return;
                }

                var ex = await response.Content.ReadAsStringAsync();
                throw new Exception(ex);
            }
        }

        public async Task<OrderStatus> GetLicenseOrderStatusAsync(string orderId)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiBaseAddress);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var path = $"api/Licenses/GetLicenseOrderStatus/{orderId}";
                    HttpResponseMessage response = await client.GetAsync(path);

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<OrderStatus>(data);
                    }

                    return OrderStatus.Error;
                }
            }
            catch (Exception)
            {
                return OrderStatus.Undefined;
            }
        }

        public async Task ChangeOrderStatusAsync(LicenseOrder licenseOrder)
        {
            await _licenseOrderRepository.UpdateOnlyPropAsync(licenseOrder, new string[] { "OrderStatus" });
        }

        #endregion

        #region License

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

        public async Task<IList<DeviceLicense>> GetDeviceLicensesByDeviceIdAsync(string deviceId)
        {
            return await _deviceLicenseRepository
                .Query()
                .Where(d => d.AppliedAt == null && d.DeviceId == deviceId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IList<DeviceLicense>> GetDeviceLicensesByOrderIdAsync(string orderId)
        {
            return await _deviceLicenseRepository
                .Query()
                .Where(d => d.AppliedAt == null && d.LicenseOrderId == orderId)
                .AsNoTracking()
                .ToListAsync();
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
                await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "HasNewLicense" });
            }
        }

        #endregion
    }
}
