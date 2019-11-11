using HES.Core.Entities;
using HES.Core.Models;
using Hideez.SDK.Communication.HES.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationSessionService
    {
        IQueryable<WorkstationSession> Query();
        IQueryable<SummaryByDayAndEmployee> SummaryByDayAndEmployeeSqlQuery(string sql);
        IQueryable<SummaryByEmployees> SummaryByEmployeesSqlQuery(string sql);
        IQueryable<SummaryByDepartments> SummaryByDepartmentsSqlQuery(string sql);
        IQueryable<SummaryByWorkstations> SummaryByWorkstationsSqlQuery(string sql);
        Task<List<WorkstationSession>> GetOpenedSessionsAsync();
        Task<int> GetOpenedSessionsCountAsync();
        Task AddOrUpdateWorkstationSession(WorkstationEventDto workstationEventDto);
        Task CloseSessionAsync(string workstationId);
    }
}