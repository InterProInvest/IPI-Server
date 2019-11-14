using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Workstation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class WorkstationService : IWorkstationService
    {
        private readonly IAsyncRepository<Workstation> _workstationRepository;
        private readonly IAsyncRepository<ProximityDevice> _proximityDeviceRepository;

        public WorkstationService(IAsyncRepository<Workstation> workstationRepository,
                                  IAsyncRepository<ProximityDevice> proximityDeviceRepository)
        {
            _workstationRepository = workstationRepository;
            _proximityDeviceRepository = proximityDeviceRepository;
        }

        #region Workstation

        public IQueryable<Workstation> QueryOfWorkstation()
        {
            return _workstationRepository.Query();
        }

        public async Task<Workstation> GetWorkstationByIdAsync(string id)
        {
            return await _workstationRepository
                .Query()
                .Include(c => c.Department.Company)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Workstation>> GetWorkstationsAsync()
        {
            return await _workstationRepository
                .Query()
                .Include(w => w.ProximityDevices)
                .Include(c => c.Department.Company)
                .ToListAsync();
        }

        public async Task<List<Workstation>> GetFilteredWorkstationsAsync(WorkstationFilter workstationFilter)
        {
            var filter = _workstationRepository
                .Query()
                .Include(w => w.ProximityDevices)
                .Include(c => c.Department.Company)
                .AsQueryable();

            if (workstationFilter.Name != null)
            {
                filter = filter.Where(w => w.Name.Contains(workstationFilter.Name));
            }
            if (workstationFilter.Domain != null)
            {
                filter = filter.Where(w => w.Domain.Contains(workstationFilter.Domain));
            }
            if (workstationFilter.ClientVersion != null)
            {
                filter = filter.Where(w => w.ClientVersion.Contains(workstationFilter.ClientVersion));
            }
            if (workstationFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Department.Company.Id == workstationFilter.CompanyId);
            }
            if (workstationFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.DepartmentId == workstationFilter.DepartmentId);
            }
            if (workstationFilter.OS != null)
            {
                filter = filter.Where(w => w.OS.Contains(workstationFilter.OS));
            }
            if (workstationFilter.IP != null)
            {
                filter = filter.Where(w => w.IP.Contains(workstationFilter.IP));
            }
            if (workstationFilter.StartDate != null && workstationFilter.EndDate != null)
            {
                filter = filter.Where(w => w.LastSeen >= workstationFilter.StartDate.Value.AddSeconds(0).AddMilliseconds(0).ToUniversalTime()
                                        && w.LastSeen <= workstationFilter.EndDate.Value.AddSeconds(59).AddMilliseconds(999).ToUniversalTime());
            }
            if (workstationFilter.RFID != null)
            {
                filter = filter.Where(w => w.RFID == workstationFilter.RFID);
            }
            if (workstationFilter.ProximityDevicesCount != null)
            {
                filter = filter.Where(w => w.ProximityDevices.Count() == workstationFilter.ProximityDevicesCount);
            }
            if (workstationFilter.Approved != null)
            {
                filter = filter.Where(w => w.Approved == workstationFilter.Approved);
            }

            return await filter
                .OrderBy(w => w.Name)
                .Take(workstationFilter.Records)
                .ToListAsync();
        }

        public Task<int> GetOnlineCountAsync()
        {
            return Task.FromResult(RemoteWorkstationConnectionsService.WorkstationsOnlineCount());
        }

        public async Task<bool> ExistAsync(Expression<Func<Workstation, bool>> predicate)
        {
            return await _workstationRepository.ExistAsync(predicate);
        }

        public async Task AddWorkstationAsync(WorkstationInfo workstationInfo)
        {
            if (workstationInfo == null)
            {
                throw new ArgumentNullException(nameof(workstationInfo));
            }

            var workstation = new Workstation()
            {
                Id = workstationInfo.Id,
                Name = workstationInfo.MachineName,
                Domain = workstationInfo.Domain,
                OS = workstationInfo.OsName,
                ClientVersion = workstationInfo.AppVersion,
                IP = workstationInfo.IP,
                LastSeen = DateTime.UtcNow,
                DepartmentId = null
            };

            await _workstationRepository.AddAsync(workstation);
        }

        public async Task UpdateWorkstationInfoAsync(WorkstationInfo workstationInfo)
        {
            if (workstationInfo == null)
            {
                throw new ArgumentNullException(nameof(workstationInfo));
            }

            var workstation = await GetWorkstationByIdAsync(workstationInfo.Id);
            if (workstation == null)
            {
                throw new Exception($"Workstation not found.");
            }

            workstation.ClientVersion = workstationInfo.AppVersion;
            workstation.OS = workstationInfo.OsName;
            workstation.IP = workstationInfo.IP;
            workstation.LastSeen = DateTime.UtcNow;

            string[] properties = { "ClientVersion", "OS", "IP", "LastSeen" };
            await _workstationRepository.UpdateOnlyPropAsync(workstation, properties);
        }

        public async Task EditWorkstationAsync(Workstation workstation)
        {
            if (workstation == null)
            {
                throw new ArgumentNullException(nameof(workstation));
            }

            string[] properties = { "DepartmentId", "RFID" };
            await _workstationRepository.UpdateOnlyPropAsync(workstation, properties);
        }

        public async Task ApproveWorkstationAsync(Workstation workstation)
        {
            if (workstation == null)
            {
                throw new ArgumentNullException(nameof(workstation));
            }

            string[] properties = { "DepartmentId", "Approved", "RFID" };
            await _workstationRepository.UpdateOnlyPropAsync(workstation, properties);
        }

        public async Task UnapproveWorkstationAsync(string workstationId)
        {
            if (workstationId == null)
            {
                throw new ArgumentNullException(nameof(workstationId));
            }

            var workstation = await GetWorkstationByIdAsync(workstationId);
            if (workstation == null)
            {
                throw new Exception("Workstation not found.");
            }

            workstation.Approved = false;
            workstation.DepartmentId = null;
            workstation.RFID = false;

            string[] properties = { "Approved", "DepartmentId", "RFID" };
            await _workstationRepository.UpdateOnlyPropAsync(workstation, properties);
        }

        public async Task<bool> GetRfidStateAsync(string workstationId)
        {
            return await _workstationRepository
                        .Query()
                        .Where(w => w.Id == workstationId)
                        .AsNoTracking()
                        .Select(s => s.RFID)
                        .FirstOrDefaultAsync();
        }

        public async Task UpdateRfidStateAsync(string workstationId)
        {
            var isEnabled = await GetRfidStateAsync(workstationId);

            await RemoteWorkstationConnectionsService.UpdateRfidIndicatorStateAsync(workstationId, isEnabled);
        }

        #endregion

        #region Proximity Device

        public IQueryable<ProximityDevice> QueryOfProximityDevice()
        {
            return _proximityDeviceRepository.Query();
        }

        public async Task<List<ProximityDevice>> GetProximityDevicesAsync(string workstationId)
        {
            return await _proximityDeviceRepository
                .Query()
                .Include(d => d.Device.Employee.Department.Company)
                .Where(d => d.WorkstationId == workstationId)
                .ToListAsync();
        }

        public async Task AddProximityDevicesAsync(string workstationId, string[] devicesIds)
        {
            if (workstationId == null)
            {
                throw new ArgumentNullException(nameof(workstationId));
            }
            if (devicesIds == null)
            {
                throw new ArgumentNullException(nameof(devicesIds));
            }

            List<ProximityDevice> proximityDevices = new List<ProximityDevice>();

            foreach (var deviceId in devicesIds)
            {
                var exists = await _proximityDeviceRepository
                .Query()
                .Where(d => d.DeviceId == deviceId)
                .Where(d => d.WorkstationId == workstationId)
                .FirstOrDefaultAsync();

                if (exists == null)
                {
                    proximityDevices.Add(new ProximityDevice
                    {
                        WorkstationId = workstationId,
                        DeviceId = deviceId,
                        LockProximity = 50,
                        UnlockProximity = 75,
                        LockTimeout = 3
                    });
                }
            }

            await _proximityDeviceRepository.AddRangeAsync(proximityDevices);
            await UpdateProximitySettingsAsync(workstationId);
        }

        public async Task AddMultipleProximityDevicesAsync(string[] workstationsIds, string[] devicesIds)
        {
            if (workstationsIds == null)
            {
                throw new ArgumentNullException(nameof(workstationsIds));
            }
            if (devicesIds == null)
            {
                throw new ArgumentNullException(nameof(devicesIds));
            }

            foreach (var workstation in workstationsIds)
            {
                await AddProximityDevicesAsync(workstation, devicesIds);
            }
        }

        public async Task EditProximityDeviceAsync(ProximityDevice proximityDevice)
        {
            if (proximityDevice == null)
            {
                throw new ArgumentNullException(nameof(proximityDevice));
            }

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

        public async Task RemoveAllProximityByDeviceIdAsync(string deviceId)
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

        public async Task<IReadOnlyList<DeviceProximitySettingsDto>> GetProximitySettingsAsync(string workstationId)
        {
            var workstation = await GetWorkstationByIdAsync(workstationId);

            if (workstation == null)
            {
                throw new Exception("Workstation not found");
            }

            var deviceProximitySettings = new List<DeviceProximitySettingsDto>();

            var proximityDevices = await _proximityDeviceRepository
                .Query()
                .Include(d => d.Device)
                .Where(d => d.WorkstationId == workstationId)
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

        #endregion
    }
}