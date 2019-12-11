using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DeviceLicenseService : IDeviceLicenseService
    {
        private readonly IAsyncRepository<DeviceLicense> _deviceLicenseRepository;

        public DeviceLicenseService(IAsyncRepository<DeviceLicense> deviceLicenseRepository)
        {
            _deviceLicenseRepository = deviceLicenseRepository;
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

        public async Task<IList<DeviceLicense>> GetDeviceLicensesByDeviceIdAsync(string deviceId)
        {
            return await _deviceLicenseRepository
                .Query()
                .Where(d => d.AppliedAt == null && d.DeviceId == deviceId)
                .ToListAsync();
        }

    }
}
