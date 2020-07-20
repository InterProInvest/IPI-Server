using HES.Core.Entities;
using HES.Core.Models;
using HES.Core.Models.Web;
using Hideez.SDK.Communication.HES.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationAuditService
    {
        IQueryable<WorkstationEvent> EventQuery();
        Task<List<WorkstationEvent>> GetWorkstationEventsAsync();
        Task<List<WorkstationEvent>> GetFilteredWorkstationEventsAsync(WorkstationEventFilter workstationEventFilter);
        Task AddEventDtoAsync(WorkstationEventDto workstationEventDto);
        Task AddPendingUnlockEventAsync(string deviceId);
        IQueryable<WorkstationSession> SessionQuery();
        Task<List<WorkstationSession>> GetWorkstationSessionsAsync();
        Task<List<WorkstationSession>> GetFilteredWorkstationSessionsAsync(WorkstationSessionFilter workstationSessionFilter);
        Task AddOrUpdateWorkstationSession(WorkstationEventDto workstationEventDto);
        Task CloseSessionAsync(string workstationId);
        Task<List<SummaryByDayAndEmployee>> GetSummaryByDayAndEmployeesAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions);
        Task<int> GetSummaryByDayAndEmployeesCountAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions);
        Task<List<SummaryByDayAndEmployee>> GetFilteredSummaryByDaysAndEmployeesAsync(SummaryFilter summaryFilter);
        Task<List<SummaryByEmployees>> GetSummaryByEmployeesAsync();
        Task<List<SummaryByEmployees>> GetFilteredSummaryByEmployeesAsync(SummaryFilter summaryFilter);
        Task<List<SummaryByDepartments>> GetSummaryByDepartmentsAsync();
        Task<List<SummaryByDepartments>> GetFilteredSummaryByDepartmentsAsync(SummaryFilter summaryFilter);
        Task<List<SummaryByWorkstations>> GetSummaryByWorkstationsAsync();
        Task<List<SummaryByWorkstations>> GetFilteredSummaryByWorkstationsAsync(SummaryFilter summaryFilter);
    }
}