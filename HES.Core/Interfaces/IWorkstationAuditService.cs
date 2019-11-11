using HES.Core.Entities;
using HES.Core.Models;
using Hideez.SDK.Communication.HES.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationAuditService
    {
        IQueryable<WorkstationEvent> QueryOfEvent();
        Task<List<WorkstationEvent>> GetWorkstationEventsAsync();
        Task<List<WorkstationEvent>> GetFilteredWorkstationEventsAsync(WorkstationEventFilter workstationEventFilter);
        Task AddEventDtoAsync(WorkstationEventDto workstationEventDto);
        Task AddPendingUnlockEventAsync(string deviceId);
        IQueryable<WorkstationSession> QueryOfSession();
        Task<List<WorkstationSession>> GetWorkstationSessionsAsync();
        Task<List<WorkstationSession>> GetOpenedSessionsAsync();
        Task<int> GetOpenedSessionsCountAsync();
        Task<List<WorkstationSession>> GetFilteredWorkstationSessionsAsync(WorkstationSessionFilter workstationSessionFilter);
        Task AddOrUpdateWorkstationSession(WorkstationEventDto workstationEventDto);
        Task CloseSessionAsync(string workstationId);
        Task<List<SummaryByDayAndEmployee>> GetSummaryByDayAndEmployeeAsync();
        Task<List<SummaryByDayAndEmployee>> GetFilteredSummaryByDaysAndEmployeesAsync(SummaryFilter summaryFilter);
        Task<List<SummaryByEmployees>> GetFilteredSummaryByEmployeesAsync(SummaryFilter summaryFilter);
        Task<List<SummaryByDepartments>> GetFilteredSummaryByDepartmentsAsync(SummaryFilter summaryFilter);
        Task<List<SummaryByWorkstations>> GetFilteredSummaryByWorkstationsAsync(SummaryFilter summaryFilter);
    }
}