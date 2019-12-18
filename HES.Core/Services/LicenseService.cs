using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class LicenseService : ILicenseService
    {
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

        public async Task SetStatusSent(LicenseOrder licenseOrder)
        {
            licenseOrder.OrderStatus = OrderStatus.Sent;
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
