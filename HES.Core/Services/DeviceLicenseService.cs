using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DeviceLicenseService : IDeviceLicenseService
    {
        private readonly IAsyncRepository<DeviceLicense> _deviceLicenseRepository;
        private readonly IAsyncRepository<Device> _deviceRepository;

        public DeviceLicenseService(IAsyncRepository<DeviceLicense> deviceLicenseRepository,
                                    IAsyncRepository<Device> deviceRepository)
        {
            _deviceLicenseRepository = deviceLicenseRepository;
            _deviceRepository = deviceRepository;
        }

        public IQueryable<DeviceLicense> Query()
        {
            return _deviceLicenseRepository.Query();
        }

        public async Task<List<DeviceLicense>> GeDeviceLicensesAsync()
        {
            return await _deviceLicenseRepository
                .Query()
                .ToListAsync();
        }

        public async Task<IList<DeviceLicense>> GetNewDeviceLicensesByDeviceIdAsync(string deviceId)
        {
            return await _deviceLicenseRepository
                .Query()
                .Where(d => d.AppliedAt == null && d.DeviceId == deviceId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task OnDeviceLicenseAppliedAsync(string deviceId, string licenseId)
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

            var existLicenses = await GetNewDeviceLicensesByDeviceIdAsync(deviceId);
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
    }
}
