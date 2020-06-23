using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Workstations;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Workstation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class WorkstationService : IWorkstationService
    {
        private readonly IAsyncRepository<Workstation> _workstationRepository;
        private readonly IAsyncRepository<WorkstationProximityVault> _workstationProximityVaultRepository;

        public WorkstationService(IAsyncRepository<Workstation> workstationRepository,
                                  IAsyncRepository<WorkstationProximityVault> workstationProximityVaultRepository)
        {
            _workstationRepository = workstationRepository;
            _workstationProximityVaultRepository = workstationProximityVaultRepository;
        }

        #region Workstation

        public IQueryable<Workstation> WorkstationQuery()
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

        public async Task<List<Workstation>> GetWorkstationsAsync(DataLoadingOptions<WorkstationFilter> dataLoadingOptions)
        {
            var query = _workstationRepository
                .Query()
                .Include(x => x.Department.Company)
                .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Name != null)
                {
                    query = query.Where(w => w.Name.Contains(dataLoadingOptions.Filter.Name, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Domain != null)
                {
                    query = query.Where(w => w.Domain.Contains(dataLoadingOptions.Filter.Domain, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.ClientVersion != null)
                {
                    query = query.Where(w => w.ClientVersion.Contains(dataLoadingOptions.Filter.ClientVersion, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    query = query.Where(x => x.Department.Company.Name.Contains(dataLoadingOptions.Filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    query = query.Where(x => x.Department.Name.Contains(dataLoadingOptions.Filter.Department, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.OS != null)
                {
                    query = query.Where(w => w.OS.Contains(dataLoadingOptions.Filter.OS, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.IP != null)
                {
                    query = query.Where(w => w.IP.Contains(dataLoadingOptions.Filter.IP, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.LastSeenStartDate != null)
                {
                    query = query.Where(w => w.LastSeen >= dataLoadingOptions.Filter.LastSeenStartDate);
                }
                if (dataLoadingOptions.Filter.LastSeenEndDate != null)
                {
                    query = query.Where(x => x.LastSeen <= dataLoadingOptions.Filter.LastSeenEndDate);
                }
                if (dataLoadingOptions.Filter.RFID != null)
                {
                    query = query.Where(x => x.RFID == dataLoadingOptions.Filter.RFID);
                }
                if (dataLoadingOptions.Filter.Approved != null)
                {
                    query = query.Where(x => x.Approved == dataLoadingOptions.Filter.Approved);
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Domain.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.ClientVersion.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Company.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.OS.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.IP.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Sort Direction
            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(Workstation.Name):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
                    break;
                case nameof(Workstation.Domain):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Domain) : query.OrderByDescending(x => x.Domain);
                    break;
                case nameof(Workstation.ClientVersion):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.ClientVersion) : query.OrderByDescending(x => x.ClientVersion);
                    break;
                case nameof(Workstation.Department.Company):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Department.Company.Name) : query.OrderByDescending(x => x.Department.Company.Name);
                    break;
                case nameof(Workstation.Department):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Department.Name) : query.OrderByDescending(x => x.Department.Name);
                    break;
                case nameof(Workstation.OS):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.OS) : query.OrderByDescending(x => x.OS);
                    break;
                case nameof(Workstation.IP):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.IP) : query.OrderByDescending(x => x.IP);
                    break;
                case nameof(Workstation.LastSeen):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.LastSeen) : query.OrderByDescending(x => x.LastSeen);
                    break;
                case nameof(Workstation.RFID):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.RFID) : query.OrderByDescending(x => x.RFID);
                    break;
                case nameof(Workstation.Approved):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Approved) : query.OrderByDescending(x => x.Approved);
                    break;
            }

            return await query.Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).ToListAsync();
        }

        public async Task<int> GetWorkstationsCountAsync(DataLoadingOptions<WorkstationFilter> dataLoadingOptions)
        {
            var query = _workstationRepository
               .Query()
               .Include(x => x.Department.Company)
               .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Name != null)
                {
                    query = query.Where(w => w.Name.Contains(dataLoadingOptions.Filter.Name, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Domain != null)
                {
                    query = query.Where(w => w.Domain.Contains(dataLoadingOptions.Filter.Domain, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.ClientVersion != null)
                {
                    query = query.Where(w => w.ClientVersion.Contains(dataLoadingOptions.Filter.ClientVersion, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    query = query.Where(x => x.Department.Company.Name.Contains(dataLoadingOptions.Filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    query = query.Where(x => x.Department.Name.Contains(dataLoadingOptions.Filter.Department, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.OS != null)
                {
                    query = query.Where(w => w.OS.Contains(dataLoadingOptions.Filter.OS, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.IP != null)
                {
                    query = query.Where(w => w.IP.Contains(dataLoadingOptions.Filter.IP, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.LastSeenStartDate != null)
                {
                    query = query.Where(w => w.LastSeen >= dataLoadingOptions.Filter.LastSeenStartDate);
                }
                if (dataLoadingOptions.Filter.LastSeenEndDate != null)
                {
                    query = query.Where(x => x.LastSeen <= dataLoadingOptions.Filter.LastSeenEndDate);
                }
                if (dataLoadingOptions.Filter.RFID != null)
                {
                    query = query.Where(x => x.RFID == dataLoadingOptions.Filter.RFID);
                }
                if (dataLoadingOptions.Filter.Approved != null)
                {
                    query = query.Where(x => x.Approved == dataLoadingOptions.Filter.Approved);
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Domain.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.ClientVersion.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Company.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.OS.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.IP.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            return await query.CountAsync();
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
                throw new ArgumentNullException(nameof(workstation));

            workstation.Approved = true;

            await _workstationRepository.UpdateAsync(workstation);
        }

        public async Task UnapproveWorkstationAsync(string workstationId)
        {
            if (workstationId == null)
                throw new ArgumentNullException(nameof(workstationId));

            var workstation = await GetWorkstationByIdAsync(workstationId);
            if (workstation == null)
                throw new Exception("Workstation not found.");

            workstation.Approved = false;
            workstation.RFID = false;
            workstation.DepartmentId = null;

            await _workstationRepository.UpdateAsync(workstation);
        }

        public async Task<bool> GetRfidStateAsync(string workstationId)
        {
            var workstation = await _workstationRepository
                        .Query()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(w => w.Id == workstationId);

            return workstation.RFID;
        }

        public async Task<bool> CheckIsApprovedAsync(string workstationId)
        {
            var workstaton = await _workstationRepository.Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == workstationId);

            if (workstaton == null)
                return false;

            if (workstaton.Approved == false)
                return false;

            return true;
        }

        public async Task DetachWorkstationsAsync(List<Workstation> workstations)
        {
            foreach (var item in workstations)
            {
                await _workstationRepository.DetachedAsync(item);
            }
        }

        public async Task UnchangedWorkstationAsync(Workstation workstation)
        {
            await _workstationRepository.UnchangedAsync(workstation);
        }


        #endregion

        #region Proximity

        public IQueryable<WorkstationProximityVault> ProximityVaultQuery()
        {
            return _workstationProximityVaultRepository.Query();
        }

        public async Task<List<WorkstationProximityVault>> GetProximityVaultsByWorkstationIdAsync(string workstationId)
        {
            return await _workstationProximityVaultRepository
                .Query()
                .Include(d => d.HardwareVault.Employee.Department.Company)
                .Where(d => d.WorkstationId == workstationId)
                .ToListAsync();
        }

        public async Task<WorkstationProximityVault> GetProximityVaultByIdAsync(string id)
        {
            return await _workstationProximityVaultRepository
                .Query()
                .Include(d => d.HardwareVault.Employee.Department.Company)
                .Include(d => d.Workstation.Department)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IList<WorkstationProximityVault>> AddProximityVaultsAsync(string workstationId, string[] vaultsIds)
        {
            if (workstationId == null)
            {
                throw new ArgumentNullException(nameof(workstationId));
            }
            if (vaultsIds == null)
            {
                throw new ArgumentNullException(nameof(vaultsIds));
            }

            List<WorkstationProximityVault> proximityVaults = new List<WorkstationProximityVault>();

            foreach (var vault in vaultsIds)
            {
                var exists = await _workstationProximityVaultRepository
                .Query()
                .Where(d => d.HardwareVaultId == vault)
                .Where(d => d.WorkstationId == workstationId)
                .FirstOrDefaultAsync();

                if (exists == null)
                {
                    proximityVaults.Add(new WorkstationProximityVault
                    {
                        WorkstationId = workstationId,
                        HardwareVaultId = vault,
                        LockProximity = 30,
                        UnlockProximity = 70,
                        LockTimeout = 5
                    });
                }
            }

            var addedVaults = await _workstationProximityVaultRepository.AddRangeAsync(proximityVaults);
            return addedVaults;
        }

        public async Task AddMultipleProximityVaultsAsync(string[] workstationsIds, string[] vaultsIds)
        {
            if (workstationsIds == null)
            {
                throw new ArgumentNullException(nameof(workstationsIds));
            }
            if (vaultsIds == null)
            {
                throw new ArgumentNullException(nameof(vaultsIds));
            }

            foreach (var workstation in workstationsIds)
            {
                await AddProximityVaultsAsync(workstation, vaultsIds);
            }
        }

        public async Task EditProximityVaultAsync(WorkstationProximityVault proximityVault)
        {
            if (proximityVault == null)
            {
                throw new ArgumentNullException(nameof(proximityVault));
            }

            string[] properties = { "LockProximity", "UnlockProximity", "LockTimeout" };
            await _workstationProximityVaultRepository.UpdateOnlyPropAsync(proximityVault, properties);
        }

        public async Task DeleteProximityVaultAsync(string proximityVaultId)
        {
            if (proximityVaultId == null)
            {
                throw new ArgumentNullException(nameof(proximityVaultId));
            }

            var proximityVault = await _workstationProximityVaultRepository.GetByIdAsync(proximityVaultId);
            if (proximityVault == null)
            {
                throw new Exception("Binding not found.");
            }

            await _workstationProximityVaultRepository.DeleteAsync(proximityVault);
        }

        public async Task DeleteRangeProximityVaultsAsync(List<WorkstationProximityVault> proximityVaults)
        {
            if (proximityVaults == null)
            {
                throw new ArgumentNullException(nameof(proximityVaults));
            }

            await _workstationProximityVaultRepository.DeleteRangeAsync(proximityVaults);
        }

        public async Task DeleteProximityByVaultIdAsync(string vaultsId)
        {
            var allProximity = await _workstationProximityVaultRepository
             .Query()
             .Where(w => w.HardwareVaultId == vaultsId)
             .ToListAsync();

            await _workstationProximityVaultRepository.DeleteRangeAsync(allProximity);
        }

        public async Task<IReadOnlyList<DeviceProximitySettingsDto>> GetProximitySettingsAsync(string workstationId)
        {
            var workstation = await GetWorkstationByIdAsync(workstationId);

            if (workstation == null)
            {
                throw new Exception("Workstation not found");
            }

            var deviceProximitySettings = new List<DeviceProximitySettingsDto>();

            var proximityDevices = await _workstationProximityVaultRepository
                .Query()
                .Include(d => d.HardwareVault)
                .Where(d => d.WorkstationId == workstationId)
                .AsNoTracking()
                .ToListAsync();

            if (workstation.Approved)
            {
                foreach (var proximity in proximityDevices)
                {
                    deviceProximitySettings.Add(new DeviceProximitySettingsDto()
                    {
                        SerialNo = proximity.HardwareVaultId,
                        Mac = proximity.HardwareVault.MAC,
                        LockProximity = proximity.LockProximity,
                        UnlockProximity = proximity.UnlockProximity,
                        LockTimeout = proximity.LockTimeout,
                    });
                }
            }

            return deviceProximitySettings;
        }

        #endregion
    }
}