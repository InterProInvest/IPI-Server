using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Hideez.SDK.Communication.HES.DTO;
using Microsoft.EntityFrameworkCore;

namespace HES.Core.Services
{
    public class ProximityDeviceService : IProximityDeviceService
    {
        private readonly IAsyncRepository<ProximityDevice> _proximityDeviceRepository;
        private readonly IWorkstationService _workstationService;

        public ProximityDeviceService(IAsyncRepository<ProximityDevice> proximityDeviceRepository,
                                                 IWorkstationService workstationService)
        {
            _proximityDeviceRepository = proximityDeviceRepository;
            _workstationService = workstationService;
        }

        public IQueryable<ProximityDevice> Query()
        {
            return _proximityDeviceRepository.Query();
        }

        public async Task AddProximityDeviceAsync(string workstationId, string[] devicesId)
        {
            if (workstationId == null)
            {
                throw new ArgumentNullException(nameof(workstationId));
            }
            if (devicesId == null)
            {
                throw new ArgumentNullException(nameof(devicesId));
            }

            List<ProximityDevice> proximityDevices = new List<ProximityDevice>();

            foreach (var deviceId in devicesId)
            {
                var exists = await _proximityDeviceRepository
                .Query()
                .Where(d => d.DeviceId == deviceId)
                .Where(d => d.WorkstationId == workstationId)
                .FirstOrDefaultAsync();

                if (exists != null)
                {
                    throw new Exception($"Device {deviceId} already exist.");
                }

                proximityDevices.Add(new ProximityDevice
                {
                    WorkstationId = workstationId,
                    DeviceId = deviceId,
                    LockProximity = 50,
                    UnlockProximity = 75,
                    LockTimeout = 3
                });
            }

            await _proximityDeviceRepository.AddRangeAsync(proximityDevices);
            await UpdateProximitySettingsAsync(workstationId);
        }

        public async Task AddMultipleProximityDevicesAsync(string[] workstationsId, string[] devicesId)
        {
            if (workstationsId == null)
            {
                throw new ArgumentNullException(nameof(workstationsId));
            }
            if (devicesId == null)
            {
                throw new ArgumentNullException(nameof(devicesId));
            }

            foreach (var workstation in workstationsId)
            {
                await AddProximityDeviceAsync(workstation, devicesId);
            }
        }

        public async Task EditProximityDeviceAsync(ProximityDevice proximityDevice)
        {
            if (proximityDevice == null)
                throw new ArgumentNullException(nameof(proximityDevice));

            string[] properties = { "LockProximity", "UnlockProximity", "LockTimeout" };
            await _proximityDeviceRepository.UpdateOnlyPropAsync(proximityDevice, properties);
            await UpdateProximitySettingsAsync(proximityDevice.WorkstationId);
        }

        public async Task DeleteProximityDeviceAsync(string proximityDeviceId)
        {
            if (proximityDeviceId == null)
            {
                throw new ArgumentNullException(nameof(proximityDeviceId));
            }

            var proximityDevice = await _proximityDeviceRepository.GetByIdAsync(proximityDeviceId);
            if (proximityDevice == null)
            {
                throw new Exception("Binding not found.");
            }

            await _proximityDeviceRepository.DeleteAsync(proximityDevice);
            await UpdateProximitySettingsAsync(proximityDevice.WorkstationId);
        }

        public async Task DeleteRangeProximityDevicesAsync(List<ProximityDevice> proximityDevices)
        {
            if (proximityDevices == null)
            {
                throw new ArgumentNullException(nameof(proximityDevices));
            }

            await _proximityDeviceRepository.DeleteRangeAsync(proximityDevices);

            foreach (var item in proximityDevices)
            {
                await UpdateProximitySettingsAsync(item.WorkstationId);
            }
        }

        public async Task<IReadOnlyList<DeviceProximitySettingsDto>> GetProximitySettingsAsync(string workstationId)
        {
            var workstation = await _workstationService
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == workstationId);

            if (workstation == null)
            {
                throw new Exception("Workstation not found");
            }

            var deviceProximitySettings = new List<DeviceProximitySettingsDto>();

            var proximityDevices = await _proximityDeviceRepository
                .Query()
                .Include(i => i.Device)
                .Where(b => b.WorkstationId == workstationId)
                .AsNoTracking()
                .ToListAsync();

            if (workstation.Approved)
            {
                foreach (var proximity in proximityDevices)
                {
                    deviceProximitySettings.Add(new DeviceProximitySettingsDto()
                    {
                        SerialNo = proximity.DeviceId,
                        Mac = proximity.Device.MAC,
                        LockProximity = proximity.LockProximity,
                        UnlockProximity = proximity.UnlockProximity,
                        LockTimeout = proximity.LockTimeout,
                    });
                }
            }

            return deviceProximitySettings;
        }

        public async Task UpdateProximitySettingsAsync(string workstationId)
        {
            var deviceProximitySettings = await GetProximitySettingsAsync(workstationId);

            await RemoteWorkstationConnectionsService.UpdateProximitySettingsAsync(workstationId, deviceProximitySettings);
        }

        public async Task RemoveAllProximityAsync(string deviceId)
        {
            var allProximity = await _proximityDeviceRepository
             .Query()
             .Where(w => w.DeviceId == deviceId)
             .ToListAsync();

            await _proximityDeviceRepository.DeleteRangeAsync(allProximity);

            foreach (var item in allProximity)
            {
                await UpdateProximitySettingsAsync(item.WorkstationId);
            }
        }
    }
}