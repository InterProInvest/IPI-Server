using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Hideez.SDK.Communication.Workstation;
using Microsoft.EntityFrameworkCore;

namespace HES.Core.Services
{
    public class WorkstationService : IWorkstationService
    {
        private readonly IAsyncRepository<Workstation> _workstationRepository;

        public WorkstationService(IAsyncRepository<Workstation> workstationRepository)
        {
            _workstationRepository = workstationRepository;
        }

        public IQueryable<Workstation> Query()
        {
            return _workstationRepository.Query();
        }

        public async Task<int> GetCountAsync()
        {
            return await _workstationRepository.GetCountAsync();
        }

        public async Task<Workstation> GetByIdAsync(dynamic id)
        {
            return await _workstationRepository.GetByIdAsync(id);
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
                throw new ArgumentNullException(nameof(workstationInfo));

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
                throw new ArgumentNullException(nameof(workstationInfo));

            var workstation = await _workstationRepository.GetByIdAsync(workstationInfo.Id);
            if (workstation == null)
                throw new Exception($"Workstation not found, ID: {workstation}");

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
                throw new ArgumentNullException(nameof(workstation));

            string[] properties = { "DepartmentId", "RFID" };
            await _workstationRepository.UpdateOnlyPropAsync(workstation, properties);
        }

        public async Task ApproveWorkstationAsync(Workstation workstation)
        {
            if (workstation == null)
                throw new ArgumentNullException(nameof(workstation));

            string[] properties = { "DepartmentId", "Approved", "RFID" };
            await _workstationRepository.UpdateOnlyPropAsync(workstation, properties);
        }

        public async Task UnapproveWorkstationAsync(string workstationId)
        {
            if (workstationId == null)
                throw new ArgumentNullException(nameof(workstationId));

            var workstation = await _workstationRepository.GetByIdAsync(workstationId);
            if (workstation == null)
                throw new Exception("Workstation not found");

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
    }
}