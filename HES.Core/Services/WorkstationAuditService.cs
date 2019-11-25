using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class WorkstationAuditService : IWorkstationAuditService
    {
        private readonly IAsyncRepository<WorkstationEvent> _workstationEventRepository;
        private readonly IAsyncRepository<WorkstationSession> _workstationSessionRepository;
        private readonly IAsyncRepository<Device> _deviceRepository;
        private readonly IAsyncRepository<Employee> _employeeRepository;
        private readonly IAsyncRepository<DeviceAccount> _deviceAccountRepository;
        private readonly IAsyncRepository<SummaryByDayAndEmployee> _summaryByDayAndEmployeeRepository;
        private readonly IAsyncRepository<SummaryByEmployees> _summaryByEmployeesRepository;
        private readonly IAsyncRepository<SummaryByDepartments> _summaryByDepartmentsRepository;
        private readonly IAsyncRepository<SummaryByWorkstations> _summaryByWorkstationsRepository;
        private readonly ILogger<WorkstationAuditService> _logger;

        public WorkstationAuditService(IAsyncRepository<WorkstationEvent> workstationEventRepository,
                                       IAsyncRepository<WorkstationSession> workstationSessionRepository,
                                       IAsyncRepository<Device> deviceRepository,
                                       IAsyncRepository<Employee> employeeRepository,
                                       IAsyncRepository<DeviceAccount> deviceAccountRepository,
                                       IAsyncRepository<SummaryByDayAndEmployee> summaryByDayAndEmployeeRepository,
                                       IAsyncRepository<SummaryByEmployees> summaryByEmployeesRepository,
                                       IAsyncRepository<SummaryByDepartments> summaryByDepartmentsRepository,
                                       IAsyncRepository<SummaryByWorkstations> summaryByWorkstationsRepository,
                                       ILogger<WorkstationAuditService> logger)
        {
            _workstationEventRepository = workstationEventRepository;
            _workstationSessionRepository = workstationSessionRepository;
            _deviceRepository = deviceRepository;
            _employeeRepository = employeeRepository;
            _deviceAccountRepository = deviceAccountRepository;
            _summaryByDayAndEmployeeRepository = summaryByDayAndEmployeeRepository;
            _summaryByEmployeesRepository = summaryByEmployeesRepository;
            _summaryByDepartmentsRepository = summaryByDepartmentsRepository;
            _summaryByWorkstationsRepository = summaryByWorkstationsRepository;
            _logger = logger;
        }

        #region Event

        public IQueryable<WorkstationEvent> EventQuery()
        {
            return _workstationEventRepository.Query();
        }

        public async Task<List<WorkstationEvent>> GetWorkstationEventsAsync()
        {
            return await _workstationEventRepository
                .Query()
                .Include(w => w.Workstation)
                .Include(w => w.Device)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.DeviceAccount)
                .OrderByDescending(w => w.Date)
                .Take(500)
                .ToListAsync();
        }

        public async Task<List<WorkstationEvent>> GetFilteredWorkstationEventsAsync(WorkstationEventFilter workstationEventFilter)
        {
            var filter = _workstationEventRepository
                 .Query()
                 .Include(w => w.Workstation)
                 .Include(w => w.Device)
                 .Include(w => w.Employee)
                 .Include(w => w.Department.Company)
                 .Include(w => w.DeviceAccount)
                 .AsQueryable();

            if (workstationEventFilter.StartDate != null && workstationEventFilter.EndDate != null)
            {
                filter = filter.Where(w => w.Date >= workstationEventFilter.StartDate.Value.Date.AddSeconds(0).AddMilliseconds(0).ToUniversalTime() &&
                                           w.Date <= workstationEventFilter.EndDate.Value.Date.AddSeconds(59).AddMilliseconds(999).ToUniversalTime());
            }
            if (workstationEventFilter.EventId != null)
            {
                filter = filter.Where(w => w.EventId == (WorkstationEventType)workstationEventFilter.EventId);
            }
            if (workstationEventFilter.SeverityId != null)
            {
                filter = filter.Where(w => w.SeverityId == (WorkstationEventSeverity)workstationEventFilter.SeverityId);
            }
            if (workstationEventFilter.Note != null)
            {
                filter = filter.Where(w => w.Note.Contains(workstationEventFilter.Note));
            }
            if (workstationEventFilter.WorkstationId != null)
            {
                filter = filter.Where(w => w.WorkstationId == workstationEventFilter.WorkstationId);
            }
            if (workstationEventFilter.UserSession != null)
            {
                filter = filter.Where(w => w.UserSession.Contains(workstationEventFilter.UserSession));
            }
            if (workstationEventFilter.DeviceId != null)
            {
                filter = filter.Where(w => w.Device.Id == workstationEventFilter.DeviceId);
            }
            if (workstationEventFilter.EmployeeId != null)
            {
                filter = filter.Where(w => w.EmployeeId == workstationEventFilter.EmployeeId);
            }
            if (workstationEventFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Department.Company.Id == workstationEventFilter.CompanyId);
            }
            if (workstationEventFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.DepartmentId == workstationEventFilter.DepartmentId);
            }
            if (workstationEventFilter.DeviceAccountId != null)
            {
                filter = filter.Where(w => w.DeviceAccountId == workstationEventFilter.DeviceAccountId);
            }
            if (workstationEventFilter.DeviceAccountTypeId != null)
            {
                filter = filter.Where(w => w.DeviceAccount.Type == (AccountType)workstationEventFilter.DeviceAccountTypeId);
            }

            return await filter
                .OrderByDescending(w => w.Date)
                .Take(workstationEventFilter.Records)
                .ToListAsync();
        }

        public async Task AddEventDtoAsync(WorkstationEventDto workstationEventDto)
        {
            if (workstationEventDto == null)
            {
                throw new ArgumentNullException(nameof(workstationEventDto));
            }

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
                    .Where(d => d.Name == workstationEventDto.AccountName &&
                                d.Login == workstationEventDto.AccountLogin &&
                                d.DeviceId == workstationEventDto.DeviceId)
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

            //await AddEventAsync(workstationEvent);
            await _workstationEventRepository.AddAsync(workstationEvent);
        }

        public async Task AddPendingUnlockEventAsync(string deviceId)
        {
            var workstationEvent = new WorkstationEvent
            {
                Date = DateTime.UtcNow,
                EventId = WorkstationEventType.DevicePendingUnlock,
                SeverityId = WorkstationEventSeverity.Info,
                DeviceId = deviceId
            };

            await _workstationEventRepository.AddAsync(workstationEvent);
        }

        #endregion

        #region Session

        public IQueryable<WorkstationSession> SessionQuery()
        {
            return _workstationSessionRepository.Query();
        }

        public async Task<List<WorkstationSession>> GetWorkstationSessionsAsync()
        {
            return await _workstationSessionRepository
                .Query()
                .Include(w => w.Workstation)
                .Include(w => w.Device)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.DeviceAccount)
                .OrderByDescending(w => w.StartDate)
                .Take(500)
                .ToListAsync();
        }

        public async Task<List<WorkstationSession>> GetFilteredWorkstationSessionsAsync(WorkstationSessionFilter workstationSessionFilter)
        {
            var filter = _workstationSessionRepository
                .Query()
                .Include(w => w.Workstation)
                .Include(w => w.Device)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.DeviceAccount)
                .AsQueryable();

            if (workstationSessionFilter.StartDate != null && workstationSessionFilter.EndDate != null)
            {
                filter = filter.Where(w => w.StartDate >= workstationSessionFilter.StartDate.Value.AddSeconds(0).AddMilliseconds(0).ToUniversalTime() &&
                                           w.EndDate <= workstationSessionFilter.EndDate.Value.AddSeconds(59).AddMilliseconds(999).ToUniversalTime());
            }
            if (workstationSessionFilter.UnlockId != null)
            {
                filter = filter.Where(w => w.UnlockedBy == (SessionSwitchSubject)workstationSessionFilter.UnlockId);
            }
            if (workstationSessionFilter.WorkstationId != null)
            {
                filter = filter.Where(w => w.WorkstationId == workstationSessionFilter.WorkstationId);
            }
            if (workstationSessionFilter.UserSession != null)
            {
                filter = filter.Where(w => w.UserSession.Contains(workstationSessionFilter.UserSession));
            }
            if (workstationSessionFilter.DeviceId != null)
            {
                filter = filter.Where(w => w.Device.Id == workstationSessionFilter.DeviceId);
            }
            if (workstationSessionFilter.EmployeeId != null)
            {
                filter = filter.Where(w => w.EmployeeId == workstationSessionFilter.EmployeeId);
            }
            if (workstationSessionFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Department.Company.Id == workstationSessionFilter.CompanyId);
            }
            if (workstationSessionFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.DepartmentId == workstationSessionFilter.DepartmentId);
            }
            if (workstationSessionFilter.DeviceAccountId != null)
            {
                filter = filter.Where(w => w.DeviceAccountId == workstationSessionFilter.DeviceAccountId);
            }
            if (workstationSessionFilter.DeviceAccountTypeId != null)
            {
                filter = filter.Where(w => w.DeviceAccount.Type == (AccountType)workstationSessionFilter.DeviceAccountTypeId);
            }

            return await filter
                .OrderByDescending(w => w.StartDate)
                .Take(workstationSessionFilter.Records)
                .ToListAsync();
        }

        public async Task AddOrUpdateWorkstationSession(WorkstationEventDto workstationEventDto)
        {
            if (workstationEventDto == null)
            {
                throw new Exception(nameof(workstationEventDto));
            }

            // On connected or unlock
            if ((workstationEventDto.EventId == WorkstationEventType.HESConnected ||
                 workstationEventDto.EventId == WorkstationEventType.ServiceStarted ||
                 workstationEventDto.EventId == WorkstationEventType.ComputerUnlock ||
                 workstationEventDto.EventId == WorkstationEventType.ComputerLogon) &&
                 workstationEventDto.WorkstationSessionId != null)
            {
                var session = await _workstationSessionRepository.Query()
                    .FirstOrDefaultAsync(w => w.Id == workstationEventDto.WorkstationSessionId);

                if (session == null)
                {
                    // Add new session
                    await AddSessionAsync(workstationEventDto);
                }
                else
                {
                    // Reopen session
                    session.EndDate = null;
                    await UpdateSessionAsync(session);

                }
            }

            // On disconnected or lock
            if ((workstationEventDto.EventId == WorkstationEventType.ComputerLock ||
                 workstationEventDto.EventId == WorkstationEventType.ComputerLogoff) &&
                 workstationEventDto.WorkstationSessionId != null)
            {
                await CloseSessionAsync(workstationEventDto.WorkstationId);
            }
        }

        public async Task CloseSessionAsync(string workstationId)
        {
            var sessions = await _workstationSessionRepository
                .Query()
                .Where(w => w.WorkstationId == workstationId && w.EndDate == null)
                .ToListAsync();

            if (sessions != null)
            {
                foreach (var session in sessions)
                {
                    session.EndDate = DateTime.UtcNow;
                    await UpdateSessionAsync(session);
                }

                var sessionsCount = sessions.Count;
                if (sessionsCount > 1)
                {
                    _logger.LogError($"[{workstationId}] {sessionsCount} sessions were closed. id:{string.Join(". id:", sessions.Select(s => s.Id).ToArray())}");
                }
            }
        }

        private async Task AddSessionAsync(WorkstationEventDto workstationEventDto)
        {
            if (workstationEventDto == null)
            {
                throw new ArgumentNullException(nameof(workstationEventDto));
            }

            Enum.TryParse(typeof(SessionSwitchSubject), workstationEventDto.Note, out object unlockMethod);
            SessionSwitchSubject unlockedBy = unlockMethod == null ? SessionSwitchSubject.NonHideez : (SessionSwitchSubject)unlockMethod;

            string employeeId = null;
            string departmentId = null;
            string deviceAccountId = null;

            if (workstationEventDto.DeviceId != null)
            {
                var device = await _deviceRepository.GetByIdAsync(workstationEventDto.DeviceId);
                var employee = await _employeeRepository.GetByIdAsync(device.EmployeeId);
                var deviceAccount = await _deviceAccountRepository
                    .Query()
                    .Where(d => d.Name == workstationEventDto.AccountName && d.Login == workstationEventDto.AccountLogin && d.DeviceId == workstationEventDto.DeviceId)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                employeeId = device?.EmployeeId;
                departmentId = employee?.DepartmentId;
                deviceAccountId = deviceAccount?.Id;
            }

            var workstationSession = new WorkstationSession()
            {
                Id = workstationEventDto.WorkstationSessionId,
                StartDate = workstationEventDto.Date,
                EndDate = null,
                UnlockedBy = unlockedBy,
                WorkstationId = workstationEventDto.WorkstationId,
                UserSession = workstationEventDto.UserSession,
                DeviceId = workstationEventDto.DeviceId,
                EmployeeId = employeeId,
                DepartmentId = departmentId,
                DeviceAccountId = deviceAccountId,
            };

            await _workstationSessionRepository.AddAsync(workstationSession);
        }

        private async Task UpdateSessionAsync(WorkstationSession workstationSession)
        {
            if (workstationSession == null)
            {
                throw new ArgumentNullException(nameof(workstationSession));
            }

            await _workstationSessionRepository.UpdateOnlyPropAsync(workstationSession, new string[] { "EndDate" });
        }

        #endregion

        #region Summary

        public async Task<List<SummaryByDayAndEmployee>> GetSummaryByDayAndEmployeesAsync()
        {
            return await _summaryByDayAndEmployeeRepository.SqlQuery
                ($@"SELECT
	                    DATE(workstationsessions.StartDate) AS Date,
	                    employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(employees.FirstName,' ',employees.LastName), 'N/A') AS Employee,
	                    companies.Id AS CompanyId,
	                    IFNULL(companies.Name, 'N/A') AS Company,
	                    departments.Id AS DepartmentId,
	                    IFNULL(departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS AvgSessionsDuration,
	                    COUNT(*) AS SessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS TotalSessionsDuration
                    FROM workstationsessions
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id
                    GROUP BY
	                    DATE(workstationsessions.StartDate),
	                    workstationsessions.EmployeeId
                    ORDER BY
	                    DATE(workstationsessions.StartDate) DESC, Employee ASC
                    LIMIT 500")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<SummaryByDayAndEmployee>> GetFilteredSummaryByDaysAndEmployeesAsync(SummaryFilter summaryFilter)
        {
            var where = string.Empty;
            List<string> parameters = new List<string>();

            if (summaryFilter.StartDate != null && summaryFilter.EndDate != null)
            {
                parameters.Add($"workstationsessions.StartDate BETWEEN '{summaryFilter.StartDate.Value.AddSeconds(0).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}' AND '{summaryFilter.EndDate.Value.AddSeconds(59).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            if (summaryFilter.EmployeeId != null)
            {
                if (summaryFilter.EmployeeId == "N/A")
                {
                    parameters.Add($"employees.Id IS NULL");
                }
                else
                {
                    parameters.Add($"employees.Id = '{summaryFilter.EmployeeId}'");
                }
            }
            if (summaryFilter.CompanyId != null)
            {
                if (summaryFilter.CompanyId == "N/A")
                {
                    parameters.Add($"companies.Id IS NULL");
                }
                else
                {
                    parameters.Add($"companies.Id = '{summaryFilter.CompanyId}'");
                }
            }
            if (summaryFilter.DepartmentId != null)
            {
                if (summaryFilter.DepartmentId == "N/A")
                {
                    parameters.Add($"departments.Id IS NULL");
                }
                else
                {
                    parameters.Add($"departments.Id = '{summaryFilter.DepartmentId}'");
                }
            }

            if (parameters.Count > 0)
            {
                where = string.Join(" AND ", parameters).Insert(0, "WHERE ");
            }

            if (summaryFilter.Records == 0)
            {
                summaryFilter.Records = 500;
            }

            return await _summaryByDayAndEmployeeRepository.SqlQuery
                ($@"SELECT
	                    DATE(workstationsessions.StartDate) AS Date,
	                    employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(employees.FirstName,' ',employees.LastName), 'N/A') AS Employee,
	                    companies.Id AS CompanyId,
	                    IFNULL(companies.Name, 'N/A') AS Company,
	                    departments.Id AS DepartmentId,
	                    IFNULL(departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS AvgSessionsDuration,
	                    COUNT(*) AS SessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS TotalSessionsDuration
                    FROM workstationsessions
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id
                {where}
                    GROUP BY
	                    DATE(workstationsessions.StartDate),
	                    workstationsessions.EmployeeId
                    ORDER BY
	                    DATE(workstationsessions.StartDate) DESC, Employee ASC
                    LIMIT {summaryFilter.Records}")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<SummaryByEmployees>> GetSummaryByEmployeesAsync()
        {
            return await _summaryByEmployeesRepository.SqlQuery
                 ($@"SELECT
	                    employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(employees.FirstName,' ',employees.LastName), 'N/A') AS Employee,
	                    companies.Id AS CompanyId,
	                    IFNULL(companies.Name, 'N/A') AS Company,
	                    departments.Id AS DepartmentId,
	                    IFNULL(departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    COUNT(DISTINCT DATE(workstationsessions.StartDate)) AS WorkingDaysCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS AvgSessionsDuration,	
	                    COUNT(*) / COUNT(DISTINCT DATE(workstationsessions.StartDate)) AS AvgSessionsCountPerDay,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate))) / COUNT(DISTINCT DATE(workstationsessions.StartDate))) AS AvgWorkingHoursPerDay
                    FROM workstationsessions
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id    
                    GROUP BY
	                    workstationsessions.EmployeeId
                    ORDER BY
	                    Employee ASC
                    LIMIT 500")
                 .AsNoTracking()
                 .ToListAsync();
        }

        public async Task<List<SummaryByEmployees>> GetFilteredSummaryByEmployeesAsync(SummaryFilter summaryFilter)
        {
            var where = string.Empty;
            List<string> parameters = new List<string>();

            if (summaryFilter.StartDate != null && summaryFilter.EndDate != null)
            {
                parameters.Add($"workstationsessions.StartDate BETWEEN '{summaryFilter.StartDate.Value.AddSeconds(0).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}' AND '{summaryFilter.EndDate.Value.AddSeconds(59).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            if (summaryFilter.CompanyId != null)
            {
                if (summaryFilter.CompanyId == "N/A")
                {
                    parameters.Add($"companies.Id IS NULL");
                }
                else
                {
                    parameters.Add($"companies.Id = '{summaryFilter.CompanyId}'");
                }
            }
            if (summaryFilter.DepartmentId != null)
            {
                if (summaryFilter.DepartmentId == "N/A")
                {
                    parameters.Add($"departments.Id IS NULL");
                }
                else
                {
                    parameters.Add($"departments.Id = '{summaryFilter.DepartmentId}'");
                }
            }

            if (parameters.Count > 0)
            {
                where = string.Join(" AND ", parameters).Insert(0, "WHERE ");
            }

            if (summaryFilter.Records == 0)
            {
                summaryFilter.Records = 500;
            }

            return await _summaryByEmployeesRepository.SqlQuery
                ($@"SELECT
	                    employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(employees.FirstName,' ',employees.LastName), 'N/A') AS Employee,
	                    companies.Id AS CompanyId,
	                    IFNULL(companies.Name, 'N/A') AS Company,
	                    departments.Id AS DepartmentId,
	                    IFNULL(departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    COUNT(DISTINCT DATE(workstationsessions.StartDate)) AS WorkingDaysCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS AvgSessionsDuration,	
	                    COUNT(*) / COUNT(DISTINCT DATE(workstationsessions.StartDate)) AS AvgSessionsCountPerDay,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate))) / COUNT(DISTINCT DATE(workstationsessions.StartDate))) AS AvgWorkingHoursPerDay
                    FROM workstationsessions
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id
                {where}
                    GROUP BY
	                    workstationsessions.EmployeeId
                    ORDER BY
	                    Employee ASC
                    LIMIT {summaryFilter.Records}")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<SummaryByDepartments>> GetSummaryByDepartmentsAsync()
        {
            return await _summaryByDepartmentsRepository.SqlQuery
                ($@"SELECT
	                    companies.Id AS CompanyId,
	                    IFNULL(companies.Name, 'N/A') AS Company,
	                    departments.Id AS DepartmentId,
	                    IFNULL(departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT IFNULL(employees.Id, 'N/A')) AS EmployeesCount,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS AvgSessionsDuration,	
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate))) / COUNT(DISTINCT IFNULL(employees.Id, 'N/A'))) AS AvgTotalDuartionByEmployee,
	                    COUNT(*) / COUNT(DISTINCT IFNULL(employees.Id, 'N/A')) AS AvgTotalSessionsCountByEmployee
                    FROM workstationsessions
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id
                    GROUP BY
	                    departments.Id
                    ORDER BY
	                    Company ASC, Department ASC
                    LIMIT 500")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<SummaryByDepartments>> GetFilteredSummaryByDepartmentsAsync(SummaryFilter summaryFilter)
        {
            var where = string.Empty;
            List<string> parameters = new List<string>();

            if (summaryFilter.StartDate != null && summaryFilter.EndDate != null)
            {
                parameters.Add($"workstationsessions.StartDate BETWEEN '{summaryFilter.StartDate.Value.AddSeconds(0).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}' AND '{summaryFilter.EndDate.Value.AddSeconds(59).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}'");
            }

            if (summaryFilter.CompanyId != null)
            {
                if (summaryFilter.CompanyId == "N/A")
                {
                    parameters.Add($"companies.Id IS NULL");
                }
                else
                {
                    parameters.Add($"companies.Id = '{summaryFilter.CompanyId}'");
                }
            }

            if (parameters.Count > 0)
            {
                where = string.Join(" AND ", parameters).Insert(0, "WHERE ");
            }

            if (summaryFilter.Records == 0)
            {
                summaryFilter.Records = 500;
            }

            return await _summaryByDepartmentsRepository.SqlQuery
                ($@"SELECT
	                    companies.Id AS CompanyId,
	                    IFNULL(companies.Name, 'N/A') AS Company,
	                    departments.Id AS DepartmentId,
	                    IFNULL(departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT IFNULL(employees.Id, 'N/A')) AS EmployeesCount,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS AvgSessionsDuration,	
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate))) / COUNT(DISTINCT IFNULL(employees.Id, 'N/A'))) AS AvgTotalDuartionByEmployee,
	                    COUNT(*) / COUNT(DISTINCT IFNULL(employees.Id, 'N/A')) AS AvgTotalSessionsCountByEmployee
                    FROM workstationsessions
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id
                {where}
                    GROUP BY
	                    departments.Id
                    ORDER BY
	                    Company ASC, Department ASC
                    LIMIT {summaryFilter.Records}")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<SummaryByWorkstations>> GetSummaryByWorkstationsAsync()
        {
            return await _summaryByWorkstationsRepository.SqlQuery
                ($@"SELECT
	                    workstations.Name AS Workstation,
	                    COUNT(DISTINCT IFNULL(companies.Id, 'N/A')) AS CompaniesCount,
	                    COUNT(DISTINCT IFNULL(departments.Id, 'N/A')) AS DepartmentsCount,
	                    COUNT(DISTINCT IFNULL(employees.Id, 'N/A')) AS EmployeesCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS AvgSessionsDuration,	
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate))) / COUNT(DISTINCT IFNULL(employees.Id, 'N/A'))) AS AvgTotalDuartionByEmployee,
	                    COUNT(*) / COUNT(DISTINCT IFNULL(employees.Id, 'N/A')) AS AvgTotalSessionsCountByEmployee
                    FROM workstationsessions
	                    LEFT JOIN workstations ON workstationsessions.WorkstationId = workstations.Id
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id 
                    GROUP BY
	                    WorkstationId
                    LIMIT 500")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<SummaryByWorkstations>> GetFilteredSummaryByWorkstationsAsync(SummaryFilter summaryFilter)
        {
            var where = string.Empty;
            List<string> parameters = new List<string>();

            if (summaryFilter.StartDate != null && summaryFilter.EndDate != null)
            {
                parameters.Add($"workstationsessions.StartDate BETWEEN '{summaryFilter.StartDate.Value.AddSeconds(0).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}' AND '{summaryFilter.EndDate.Value.AddSeconds(59).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            if (summaryFilter.EmployeeId != null)
            {
                if (summaryFilter.EmployeeId == "N/A")
                {
                    parameters.Add($"employees.Id IS NULL");
                }
                else
                {
                    parameters.Add($"employees.Id = '{summaryFilter.EmployeeId}'");
                }
            }
            if (summaryFilter.CompanyId != null)
            {
                if (summaryFilter.CompanyId == "N/A")
                {
                    parameters.Add($"companies.Id IS NULL");
                }
                else
                {
                    parameters.Add($"companies.Id = '{summaryFilter.CompanyId}'");
                }
            }
            if (summaryFilter.DepartmentId != null)
            {
                if (summaryFilter.DepartmentId == "N/A")
                {
                    parameters.Add($"departments.Id IS NULL");
                }
                else
                {
                    parameters.Add($"departments.Id = '{summaryFilter.DepartmentId}'");
                }
            }

            if (parameters.Count > 0)
            {
                where = string.Join(" AND ", parameters).Insert(0, "WHERE ");
            }

            if (summaryFilter.Records == 0)
            {
                summaryFilter.Records = 500;
            }

            return await _summaryByWorkstationsRepository.SqlQuery
                ($@"SELECT
	                    workstations.Name AS Workstation,
	                    COUNT(DISTINCT IFNULL(companies.Id, 'N/A')) AS CompaniesCount,
	                    COUNT(DISTINCT IFNULL(departments.Id, 'N/A')) AS DepartmentsCount,
	                    COUNT(DISTINCT IFNULL(employees.Id, 'N/A')) AS EmployeesCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate)))) AS AvgSessionsDuration,	
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndDate, NOW()), workstationsessions.StartDate))) / COUNT(DISTINCT IFNULL(employees.Id, 'N/A'))) AS AvgTotalDuartionByEmployee,
	                    COUNT(*) / COUNT(DISTINCT IFNULL(employees.Id, 'N/A')) AS AvgTotalSessionsCountByEmployee
                    FROM workstationsessions
	                    LEFT JOIN workstations ON workstationsessions.WorkstationId = workstations.Id
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id
                {where}
                    GROUP BY
	                    WorkstationId
                    LIMIT {summaryFilter.Records}")
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion
    }
}
