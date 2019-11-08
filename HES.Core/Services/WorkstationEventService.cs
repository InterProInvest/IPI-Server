using HES.Core.Entities;
using HES.Core.Interfaces;
using Hideez.SDK.Communication.HES.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class WorkstationEventService : IWorkstationEventService
    {
        private readonly IAsyncRepository<WorkstationEvent> _workstationEventRepository;
        private readonly IAsyncRepository<Device> _deviceRepository;
        private readonly IAsyncRepository<Employee> _employeeRepository;
        private readonly IAsyncRepository<DeviceAccount> _deviceAccountRepository;
        private readonly ILogger<WorkstationEventService> _logger;

        public WorkstationEventService(IAsyncRepository<WorkstationEvent> workstationEventRepository,
                                       IAsyncRepository<Device> deviceRepository,
                                       IAsyncRepository<Employee> employeeRepository,
                                       IAsyncRepository<DeviceAccount> deviceAccountRepository,
                                       ILogger<WorkstationEventService> logger)
        {
            _workstationEventRepository = workstationEventRepository;
            _deviceRepository = deviceRepository;
            _employeeRepository = employeeRepository;
            _deviceAccountRepository = deviceAccountRepository;
            _logger = logger;
        }

        public IQueryable<WorkstationEvent> Query()
        {
            return _workstationEventRepository.Query();
        }

        public async Task AddEventAsync(WorkstationEvent workstationEvent)
        {
            if (workstationEvent == null)
                throw new ArgumentNullException(nameof(workstationEvent));

            await _workstationEventRepository.AddAsync(workstationEvent);
        }

        public async Task AddEventDtoAsync(WorkstationEventDto workstationEventDto)
        {
            if (workstationEventDto == null)
                throw new ArgumentNullException(nameof(workstationEventDto));

            var exist = await _workstationEventRepository.Query().AsNoTracking().AnyAsync(d => d.Id == workstationEventDto.Id);
            if (exist)
            {
                _logger.LogWarning($"[DUPLICATE EVENT][{workstationEventDto.WorkstationId}] EventId:{workstationEventDto.Id}, Session:{workstationEventDto.UserSession}, Event:{workstationEventDto.EventId}");
                return;
            }

            string employeeId = null;
            string departmentId = null;
            string deviceAccountId = null;

            if (workstationEventDto.DeviceId != null)
            {
                var device = await _deviceRepository.GetByIdAsync(workstationEventDto.DeviceId);
                var employee = await _employeeRepository.GetByIdAsync(device?.EmployeeId);
                var deviceAccount = await _deviceAccountRepository
                    .Query()
                    .Where(d => d.Name == workstationEventDto.AccountName && d.Login == workstationEventDto.AccountLogin && d.DeviceId == workstationEventDto.DeviceId)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                employeeId = device?.EmployeeId;
                departmentId = employee?.DepartmentId;
                deviceAccountId = deviceAccount?.Id;
            }

            var workstationEvent = new WorkstationEvent()
            {
                Id = workstationEventDto.Id,
                Date = workstationEventDto.Date,
                EventId = workstationEventDto.EventId,
                SeverityId = workstationEventDto.SeverityId,
                Note = workstationEventDto.Note,
                WorkstationId = workstationEventDto.WorkstationId,
                UserSession = workstationEventDto.UserSession,
                DeviceId = workstationEventDto.DeviceId,
                EmployeeId = employeeId,
                DepartmentId = departmentId,
                DeviceAccountId = deviceAccountId,
            };

            await AddEventAsync(workstationEvent);
        }
    }
}