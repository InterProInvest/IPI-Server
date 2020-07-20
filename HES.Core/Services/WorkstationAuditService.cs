using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models;
using HES.Core.Models.Web;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class WorkstationAuditService : IWorkstationAuditService
    {
        private readonly IAsyncRepository<WorkstationEvent> _workstationEventRepository;
        private readonly IAsyncRepository<WorkstationSession> _workstationSessionRepository;
        private readonly IAsyncRepository<Workstation> _workstationRepository;
        private readonly IAsyncRepository<HardwareVault> _hardwareVaultRepository;
        private readonly IAsyncRepository<Employee> _employeeRepository;
        private readonly IAsyncRepository<Account> _accountRepository;
        private readonly IAsyncRepository<SummaryByDayAndEmployee> _summaryByDayAndEmployeeRepository;
        private readonly IAsyncRepository<SummaryByEmployees> _summaryByEmployeesRepository;
        private readonly IAsyncRepository<SummaryByDepartments> _summaryByDepartmentsRepository;
        private readonly IAsyncRepository<SummaryByWorkstations> _summaryByWorkstationsRepository;
        private readonly ILogger<WorkstationAuditService> _logger;

        public WorkstationAuditService(IAsyncRepository<WorkstationEvent> workstationEventRepository,
                                       IAsyncRepository<WorkstationSession> workstationSessionRepository,
                                       IAsyncRepository<Workstation> workstationRepository,
                                       IAsyncRepository<HardwareVault> hardwareVaultRepository,
                                       IAsyncRepository<Employee> employeeRepository,
                                       IAsyncRepository<Account> accountRepository,
                                       IAsyncRepository<SummaryByDayAndEmployee> summaryByDayAndEmployeeRepository,
                                       IAsyncRepository<SummaryByEmployees> summaryByEmployeesRepository,
                                       IAsyncRepository<SummaryByDepartments> summaryByDepartmentsRepository,
                                       IAsyncRepository<SummaryByWorkstations> summaryByWorkstationsRepository,
                                       ILogger<WorkstationAuditService> logger)
        {
            _workstationEventRepository = workstationEventRepository;
            _workstationSessionRepository = workstationSessionRepository;
            _workstationRepository = workstationRepository;
            _hardwareVaultRepository = hardwareVaultRepository;
            _employeeRepository = employeeRepository;
            _accountRepository = accountRepository;
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
                .Include(w => w.HardwareVault)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.Account)
                .OrderByDescending(w => w.Date)
                .Take(500)
                .ToListAsync();
        }

        public async Task<List<WorkstationEvent>> GetFilteredWorkstationEventsAsync(WorkstationEventFilter workstationEventFilter)
        {
            var filter = _workstationEventRepository
                 .Query()
                 .Include(w => w.Workstation)
                 .Include(w => w.HardwareVault)
                 .Include(w => w.Employee)
                 .Include(w => w.Department.Company)
                 .Include(w => w.Account)
                 .AsQueryable();

            if (workstationEventFilter.StartDate != null)
            {
                filter = filter.Where(w => w.Date >= workstationEventFilter.StartDate.Value.AddSeconds(0).AddMilliseconds(0).ToUniversalTime());
            }
            if (workstationEventFilter.EndDate != null)
            {
                filter = filter.Where(w => w.Date <= workstationEventFilter.EndDate.Value.AddSeconds(59).AddMilliseconds(999).ToUniversalTime());
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
                filter = filter.Where(w => w.HardwareVault.Id == workstationEventFilter.DeviceId);
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
                filter = filter.Where(w => w.AccountId == workstationEventFilter.DeviceAccountId);
            }
            if (workstationEventFilter.DeviceAccountTypeId != null)
            {
                filter = filter.Where(w => w.Account.Type == (AccountType)workstationEventFilter.DeviceAccountTypeId);
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

            var workstation = await _workstationRepository.GetByIdAsync(workstationEventDto.WorkstationId);
            if (workstation == null)
            {
                throw new Exception($"Workstation not found, ID:{workstationEventDto.WorkstationId}");
            }

            Employee employee = null;
            Account account = null;

            if (workstationEventDto.DeviceId != null)
            {
                var hardwareVault = await _hardwareVaultRepository.GetByIdAsync(workstationEventDto.DeviceId);
                if (hardwareVault != null)
                {
                    employee = await _employeeRepository.GetByIdAsync(hardwareVault.EmployeeId);

                    account = await _accountRepository
                        .Query()
                        .Where(d => d.Name == workstationEventDto.AccountName &&
                                    d.Login == workstationEventDto.AccountLogin &&
                                    d.EmployeeId == hardwareVault.EmployeeId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                }
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
                HardwareVaultId = workstationEventDto.DeviceId,
                EmployeeId = employee?.Id,
                DepartmentId = employee?.DepartmentId,
                AccountId = account?.Id,
            };

            await _workstationEventRepository.AddAsync(workstationEvent);
        }

        public async Task AddPendingUnlockEventAsync(string deviceId)
        {
            var workstationEvent = new WorkstationEvent
            {
                Date = DateTime.UtcNow,
                EventId = WorkstationEventType.DevicePendingUnlock,
                SeverityId = WorkstationEventSeverity.Info,
                HardwareVaultId = deviceId
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
                .Include(w => w.HardwareVault)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.Account)
                .OrderByDescending(w => w.StartDate)
                .Take(500)
                .ToListAsync();
        }

        public async Task<List<WorkstationSession>> GetFilteredWorkstationSessionsAsync(WorkstationSessionFilter workstationSessionFilter)
        {
            var filter = _workstationSessionRepository
                .Query()
                .Include(w => w.Workstation)
                .Include(w => w.HardwareVault)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.Account)
                .AsQueryable();

            if (workstationSessionFilter.StartDate != null)
            {
                filter = filter.Where(w => w.StartDate >= workstationSessionFilter.StartDate.Value.AddSeconds(0).AddMilliseconds(0).ToUniversalTime());
            }
            if (workstationSessionFilter.EndDate != null)
            {
                filter = filter.Where(w => w.EndDate <= workstationSessionFilter.EndDate.Value.AddSeconds(59).AddMilliseconds(999).ToUniversalTime());
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
                filter = filter.Where(w => w.HardwareVault.Id == workstationSessionFilter.DeviceId);
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
                filter = filter.Where(w => w.AccountId == workstationSessionFilter.DeviceAccountId);
            }
            if (workstationSessionFilter.DeviceAccountTypeId != null)
            {
                filter = filter.Where(w => w.Account.Type == (AccountType)workstationSessionFilter.DeviceAccountTypeId);
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

            var workstation = await _workstationRepository.GetByIdAsync(workstationEventDto.WorkstationId);
            if (workstation == null)
            {
                throw new Exception($"Workstation not found, ID:{workstationEventDto.WorkstationId}");
            }

            Enum.TryParse(typeof(SessionSwitchSubject), workstationEventDto.Note, out object unlockMethod);
            SessionSwitchSubject unlockedBy = unlockMethod == null ? SessionSwitchSubject.NonHideez : (SessionSwitchSubject)unlockMethod;

            Employee employee = null;
            Account account = null;

            if (workstationEventDto.DeviceId != null)
            {
                var hardwareVault = await _hardwareVaultRepository.GetByIdAsync(workstationEventDto.DeviceId);
                if (hardwareVault != null)
                {
                    employee = await _employeeRepository.GetByIdAsync(hardwareVault.EmployeeId);

                    account = await _accountRepository
                        .Query()
                        .Where(d => d.Name == workstationEventDto.AccountName &&
                                    d.Login == workstationEventDto.AccountLogin &&
                                    d.EmployeeId == hardwareVault.EmployeeId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                }
            }

            var workstationSession = new WorkstationSession()
            {
                Id = workstationEventDto.WorkstationSessionId,
                StartDate = workstationEventDto.Date,
                EndDate = null,
                UnlockedBy = unlockedBy,
                WorkstationId = workstationEventDto.WorkstationId,
                UserSession = workstationEventDto.UserSession,
                HardwareVaultId = workstationEventDto.DeviceId,
                EmployeeId = employee?.Id,
                DepartmentId = employee?.DepartmentId,
                AccountId = account?.Id,
            };

            await _workstationSessionRepository.AddAsync(workstationSession);
        }

        private async Task UpdateSessionAsync(WorkstationSession workstationSession)
        {
            if (workstationSession == null)
                throw new ArgumentNullException(nameof(workstationSession));

            await _workstationSessionRepository.UpdateOnlyPropAsync(workstationSession, new string[] { "EndDate" });
        }

        #endregion

        #region Summary

        public async Task<List<SummaryByDayAndEmployee>> GetSummaryByDayAndEmployeesAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions)
        {
            var having = string.Empty;
            var searchParameter = string.Empty;
            var filterParameters = new List<string>();
            var orderby = string.Empty;

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.StartDate != null)
                {
                    filterParameters.Add($"Date >= '{dataLoadingOptions.Filter.StartDate.Value.ToString("yyyy-MM-dd")}'");
                }
                if (dataLoadingOptions.Filter.EndDate != null)
                {
                    filterParameters.Add($"Date <= '{dataLoadingOptions.Filter.EndDate.Value.ToString("yyyy-MM-dd")}'");
                }
                if (dataLoadingOptions.Filter.Employee != null)
                {
                    filterParameters.Add($"Employee LIKE '%{dataLoadingOptions.Filter.Employee}%'");       
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    filterParameters.Add($"Company LIKE '%{dataLoadingOptions.Filter.Company}%'");
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    filterParameters.Add($"Department LIKE '%{dataLoadingOptions.Filter.Department}%'");                   
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();
                searchParameter = $"Date LIKE '%{dataLoadingOptions.SearchText}%' OR Employee LIKE '%{dataLoadingOptions.SearchText}%' OR Company LIKE '%{dataLoadingOptions.SearchText}%' OR Department LIKE '%{dataLoadingOptions.SearchText}%'";
            }

            // Sort Direction
            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(SummaryByDayAndEmployee.Date):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY Date ASC, Employee ASC" : "ORDER BY Date DESC, Employee ASC";
                    break;
                case nameof(SummaryByDayAndEmployee.Employee):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY Employee ASC" : "ORDER BY Employee DESC";
                    break;
                case nameof(SummaryByDayAndEmployee.Company):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY Company ASC" : "ORDER BY Company DESC";
                    break;
                case nameof(SummaryByDayAndEmployee.Department):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY Department ASC" : "ORDER BY Department DESC";
                    break;
                case nameof(SummaryByDayAndEmployee.WorkstationsCount):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY WorkstationsCount ASC" : "ORDER BY WorkstationsCount DESC";
                    break;
                case nameof(SummaryByDayAndEmployee.AvgSessionsDuration):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY AvgSessionsDuration ASC" : "ORDER BY AvgSessionsDuration DESC";
                    break;
                case nameof(SummaryByDayAndEmployee.SessionsCount):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY SessionsCount ASC" : "ORDER BY SessionsCount DESC";
                    break;
                case nameof(SummaryByDayAndEmployee.TotalSessionsDuration):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY TotalSessionsDuration ASC" : "ORDER BY TotalSessionsDuration DESC";
                    break;
            }

            if (filterParameters.Count > 0 && searchParameter == string.Empty)
            {
                having = string.Join(" AND ", filterParameters).Insert(0, "HAVING ");
            }
            else if (filterParameters.Count == 0 && searchParameter != string.Empty)
            {
                having = $"HAVING {searchParameter}";
            }
            else if (filterParameters.Count > 0 && searchParameter != string.Empty)
            {
                var filter = string.Join(" AND ", filterParameters);
                having = $"HAVING ({filter}) AND ({searchParameter})";
            }

            return await _summaryByDayAndEmployeeRepository.SqlQuery
                ($@"SELECT
	                    DATE(WorkstationSessions.StartDate) AS Date,
	                    Employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(Employees.FirstName,' ',Employees.LastName), 'N/A') AS Employee,
	                    Companies.Id AS CompanyId,
	                    IFNULL(Companies.Name, 'N/A') AS Company,
	                    Departments.Id AS DepartmentId,
	                    IFNULL(Departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS AvgSessionsDuration,
	                    COUNT(*) AS SessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS TotalSessionsDuration
                    FROM WorkstationSessions
	                    LEFT JOIN Employees ON WorkstationSessions.EmployeeId = Employees.Id
	                    LEFT JOIN Departments ON Employees.DepartmentId = Departments.Id
	                    LEFT JOIN Companies ON Departments.CompanyId = Companies.Id
                    GROUP BY
	                    DATE(WorkstationSessions.StartDate),
	                    WorkstationSessions.EmployeeId
                {having}
                {orderby}
                    LIMIT {dataLoadingOptions.Take} OFFSET {dataLoadingOptions.Skip}")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetSummaryByDayAndEmployeesCountAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions)
        {
            var having = string.Empty;
            var searchParameter = string.Empty;
            var filterParameters = new List<string>();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.StartDate != null)
                {
                    filterParameters.Add($"Date >= '{dataLoadingOptions.Filter.StartDate.Value.ToString("yyyy-MM-dd")}'");
                }
                if (dataLoadingOptions.Filter.EndDate != null)
                {
                    filterParameters.Add($"Date <= '{dataLoadingOptions.Filter.EndDate.Value.ToString("yyyy-MM-dd")}'");
                }
                if (dataLoadingOptions.Filter.Employee != null)
                {
                    filterParameters.Add($"Employee LIKE '%{dataLoadingOptions.Filter.Employee}%'");
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    filterParameters.Add($"Company LIKE '%{dataLoadingOptions.Filter.Company}%'");
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    filterParameters.Add($"Department LIKE '%{dataLoadingOptions.Filter.Department}%'");
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();
                searchParameter = $"Date LIKE '%{dataLoadingOptions.SearchText}%' OR Employee LIKE '%{dataLoadingOptions.SearchText}%' OR Company LIKE '%{dataLoadingOptions.SearchText}%' OR Department LIKE '%{dataLoadingOptions.SearchText}%'";
            }

            if (filterParameters.Count > 0 && searchParameter == string.Empty)
            {
                having = string.Join(" AND ", filterParameters).Insert(0, "HAVING ");
            }
            else if (filterParameters.Count == 0 && searchParameter != string.Empty)
            {
                having = $"HAVING {searchParameter}";
            }
            else if (filterParameters.Count > 0 && searchParameter != string.Empty)
            {
                var filter = string.Join(" AND ", filterParameters);
                having = $"HAVING ({filter}) AND ({searchParameter})";
            }

            return await _summaryByDayAndEmployeeRepository.SqlQuery
                ($@"SELECT
	                    DATE(WorkstationSessions.StartDate) AS Date,
	                    Employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(Employees.FirstName,' ',Employees.LastName), 'N/A') AS Employee,
	                    Companies.Id AS CompanyId,
	                    IFNULL(Companies.Name, 'N/A') AS Company,
	                    Departments.Id AS DepartmentId,
	                    IFNULL(Departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS AvgSessionsDuration,
	                    COUNT(*) AS SessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS TotalSessionsDuration
                    FROM WorkstationSessions
	                    LEFT JOIN Employees ON WorkstationSessions.EmployeeId = Employees.Id
	                    LEFT JOIN Departments ON Employees.DepartmentId = Departments.Id
	                    LEFT JOIN Companies ON Departments.CompanyId = Companies.Id
                    GROUP BY
	                    DATE(WorkstationSessions.StartDate),
	                    WorkstationSessions.EmployeeId
                {having}
                    ORDER BY
	                    DATE(WorkstationSessions.StartDate) DESC, Employee ASC")
                .CountAsync();
        }

        public async Task<List<SummaryByDayAndEmployee>> GetFilteredSummaryByDaysAndEmployeesAsync(SummaryFilter summaryFilter)
        {
            var where = string.Empty;
            List<string> parameters = new List<string>();

            if (summaryFilter.StartDate != null && summaryFilter.EndDate != null)
            {
                parameters.Add($"WorkstationSessions.StartDate >= '{summaryFilter.StartDate.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            if (summaryFilter.EndDate != null)
            {
                parameters.Add($"WorkstationSessions.EndDate <= '{summaryFilter.EndDate.Value.AddSeconds(59).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            if (summaryFilter.Employee != null)
            {
                if (summaryFilter.Employee == "N/A")
                {
                    parameters.Add($"Employees.Id IS NULL");
                }
                else
                {
                    parameters.Add($"Employees.Id = '{summaryFilter.Employee}'");
                }
            }
            if (summaryFilter.Company != null)
            {
                if (summaryFilter.Company == "N/A")
                {
                    parameters.Add($"Companies.Id IS NULL");
                }
                else
                {
                    parameters.Add($"Companies.Id = '{summaryFilter.Company}'");
                }
            }
            if (summaryFilter.Department != null)
            {
                if (summaryFilter.Department == "N/A")
                {
                    parameters.Add($"Departments.Id IS NULL");
                }
                else
                {
                    parameters.Add($"Departments.Id = '{summaryFilter.Department}'");
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
	                    DATE(WorkstationSessions.StartDate) AS Date,
	                    Employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(Employees.FirstName,' ',Employees.LastName), 'N/A') AS Employee,
	                    Companies.Id AS CompanyId,
	                    IFNULL(Companies.Name, 'N/A') AS Company,
	                    Departments.Id AS DepartmentId,
	                    IFNULL(Departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS AvgSessionsDuration,
	                    COUNT(*) AS SessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS TotalSessionsDuration
                    FROM WorkstationSessions
	                    LEFT JOIN Employees ON WorkstationSessions.EmployeeId = Employees.Id
	                    LEFT JOIN Departments ON Employees.DepartmentId = Departments.Id
	                    LEFT JOIN Companies ON Departments.CompanyId = Companies.Id
                {where}
                    GROUP BY
	                    DATE(WorkstationSessions.StartDate),
	                    WorkstationSessions.EmployeeId
                    ORDER BY
	                    DATE(WorkstationSessions.StartDate) DESC, Employee ASC
                    LIMIT {summaryFilter.Records}")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<SummaryByEmployees>> GetSummaryByEmployeesAsync()
        {
            return await _summaryByEmployeesRepository.SqlQuery
                 ($@"SELECT
	                    Employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(Employees.FirstName,' ',Employees.LastName), 'N/A') AS Employee,
	                    Companies.Id AS CompanyId,
	                    IFNULL(Companies.Name, 'N/A') AS Company,
	                    Departments.Id AS DepartmentId,
	                    IFNULL(Departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    COUNT(DISTINCT DATE(WorkstationSessions.StartDate)) AS WorkingDaysCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS AvgSessionsDuration,	
	                    COUNT(*) / COUNT(DISTINCT DATE(WorkstationSessions.StartDate)) AS AvgSessionsCountPerDay,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate))) / COUNT(DISTINCT DATE(WorkstationSessions.StartDate))) AS AvgWorkingHoursPerDay
                    FROM WorkstationSessions
	                    LEFT JOIN Employees ON WorkstationSessions.EmployeeId = Employees.Id
	                    LEFT JOIN Departments ON Employees.DepartmentId = Departments.Id
	                    LEFT JOIN Companies ON Departments.CompanyId = Companies.Id    
                    GROUP BY
	                    WorkstationSessions.EmployeeId
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
                parameters.Add($"WorkstationSessions.StartDate BETWEEN '{summaryFilter.StartDate.Value.AddSeconds(0).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}' AND '{summaryFilter.EndDate.Value.AddSeconds(59).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            if (summaryFilter.Company != null)
            {
                if (summaryFilter.Company == "N/A")
                {
                    parameters.Add($"Companies.Id IS NULL");
                }
                else
                {
                    parameters.Add($"Companies.Id = '{summaryFilter.Company}'");
                }
            }
            if (summaryFilter.Department != null)
            {
                if (summaryFilter.Department == "N/A")
                {
                    parameters.Add($"Departments.Id IS NULL");
                }
                else
                {
                    parameters.Add($"Departments.Id = '{summaryFilter.Department}'");
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
	                    Employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(Employees.FirstName,' ',Employees.LastName), 'N/A') AS Employee,
	                    Companies.Id AS CompanyId,
	                    IFNULL(Companies.Name, 'N/A') AS Company,
	                    Departments.Id AS DepartmentId,
	                    IFNULL(Departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    COUNT(DISTINCT DATE(WorkstationSessions.StartDate)) AS WorkingDaysCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS AvgSessionsDuration,	
	                    COUNT(*) / COUNT(DISTINCT DATE(WorkstationSessions.StartDate)) AS AvgSessionsCountPerDay,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate))) / COUNT(DISTINCT DATE(WorkstationSessions.StartDate))) AS AvgWorkingHoursPerDay
                    FROM WorkstationSessions
	                    LEFT JOIN Employees ON WorkstationSessions.EmployeeId = Employees.Id
	                    LEFT JOIN Departments ON Employees.DepartmentId = Departments.Id
	                    LEFT JOIN Companies ON Departments.CompanyId = Companies.Id
                {where}
                    GROUP BY
	                    WorkstationSessions.EmployeeId
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
	                    Companies.Id AS CompanyId,
	                    IFNULL(Companies.Name, 'N/A') AS Company,
	                    Departments.Id AS DepartmentId,
	                    IFNULL(Departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT IFNULL(Employees.Id, 'N/A')) AS EmployeesCount,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS AvgSessionsDuration,	
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate))) / COUNT(DISTINCT IFNULL(Employees.Id, 'N/A'))) AS AvgTotalDuartionByEmployee,
	                    COUNT(*) / COUNT(DISTINCT IFNULL(Employees.Id, 'N/A')) AS AvgTotalSessionsCountByEmployee
                    FROM WorkstationSessions
	                    LEFT JOIN Employees ON WorkstationSessions.EmployeeId = Employees.Id
	                    LEFT JOIN Departments ON Employees.DepartmentId = Departments.Id
	                    LEFT JOIN Companies ON Departments.CompanyId = Companies.Id
                    GROUP BY
	                    Departments.Id
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
                parameters.Add($"WorkstationSessions.StartDate BETWEEN '{summaryFilter.StartDate.Value.AddSeconds(0).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}' AND '{summaryFilter.EndDate.Value.AddSeconds(59).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}'");
            }

            if (summaryFilter.Company != null)
            {
                if (summaryFilter.Company == "N/A")
                {
                    parameters.Add($"Companies.Id IS NULL");
                }
                else
                {
                    parameters.Add($"Companies.Id = '{summaryFilter.Company}'");
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
	                    Companies.Id AS CompanyId,
	                    IFNULL(Companies.Name, 'N/A') AS Company,
	                    Departments.Id AS DepartmentId,
	                    IFNULL(Departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT IFNULL(Employees.Id, 'N/A')) AS EmployeesCount,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS AvgSessionsDuration,	
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate))) / COUNT(DISTINCT IFNULL(Employees.Id, 'N/A'))) AS AvgTotalDuartionByEmployee,
	                    COUNT(*) / COUNT(DISTINCT IFNULL(Employees.Id, 'N/A')) AS AvgTotalSessionsCountByEmployee
                    FROM WorkstationSessions
	                    LEFT JOIN Employees ON WorkstationSessions.EmployeeId = Employees.Id
	                    LEFT JOIN Departments ON Employees.DepartmentId = Departments.Id
	                    LEFT JOIN Companies ON Departments.CompanyId = Companies.Id
                {where}
                    GROUP BY
	                    Departments.Id
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
	                    Workstations.Name AS Workstation,
	                    COUNT(DISTINCT IFNULL(Companies.Id, 'N/A')) AS CompaniesCount,
	                    COUNT(DISTINCT IFNULL(Departments.Id, 'N/A')) AS DepartmentsCount,
	                    COUNT(DISTINCT IFNULL(Employees.Id, 'N/A')) AS EmployeesCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS AvgSessionsDuration,	
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate))) / COUNT(DISTINCT IFNULL(Employees.Id, 'N/A'))) AS AvgTotalDuartionByEmployee,
	                    COUNT(*) / COUNT(DISTINCT IFNULL(Employees.Id, 'N/A')) AS AvgTotalSessionsCountByEmployee
                    FROM WorkstationSessions
	                    LEFT JOIN Workstations ON WorkstationSessions.WorkstationId = Workstations.Id
	                    LEFT JOIN Employees ON WorkstationSessions.EmployeeId = Employees.Id
	                    LEFT JOIN Departments ON Employees.DepartmentId = Departments.Id
	                    LEFT JOIN Companies ON Departments.CompanyId = Companies.Id 
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
                parameters.Add($"WorkstationSessions.StartDate BETWEEN '{summaryFilter.StartDate.Value.AddSeconds(0).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}' AND '{summaryFilter.EndDate.Value.AddSeconds(59).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            if (summaryFilter.Employee != null)
            {
                if (summaryFilter.Employee == "N/A")
                {
                    parameters.Add($"Employees.Id IS NULL");
                }
                else
                {
                    parameters.Add($"Employees.Id = '{summaryFilter.Employee}'");
                }
            }
            if (summaryFilter.Company != null)
            {
                if (summaryFilter.Company == "N/A")
                {
                    parameters.Add($"Companies.Id IS NULL");
                }
                else
                {
                    parameters.Add($"Companies.Id = '{summaryFilter.Company}'");
                }
            }
            if (summaryFilter.Department != null)
            {
                if (summaryFilter.Department == "N/A")
                {
                    parameters.Add($"Departments.Id IS NULL");
                }
                else
                {
                    parameters.Add($"Departments.Id = '{summaryFilter.Department}'");
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
	                    Workstations.Name AS Workstation,
	                    COUNT(DISTINCT IFNULL(Companies.Id, 'N/A')) AS CompaniesCount,
	                    COUNT(DISTINCT IFNULL(Departments.Id, 'N/A')) AS DepartmentsCount,
	                    COUNT(DISTINCT IFNULL(Employees.Id, 'N/A')) AS EmployeesCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS AvgSessionsDuration,	
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate))) / COUNT(DISTINCT IFNULL(Employees.Id, 'N/A'))) AS AvgTotalDuartionByEmployee,
	                    COUNT(*) / COUNT(DISTINCT IFNULL(Employees.Id, 'N/A')) AS AvgTotalSessionsCountByEmployee
                    FROM WorkstationSessions
	                    LEFT JOIN Workstations ON WorkstationSessions.WorkstationId = Workstations.Id
	                    LEFT JOIN Employees ON WorkstationSessions.EmployeeId = Employees.Id
	                    LEFT JOIN Departments ON Employees.DepartmentId = Departments.Id
	                    LEFT JOIN Companies ON Departments.CompanyId = Companies.Id
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