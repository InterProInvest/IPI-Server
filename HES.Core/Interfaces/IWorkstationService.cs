using HES.Core.Entities;
using Hideez.SDK.Communication.Workstation;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationService
    {
        IQueryable<Workstation> Query();
        Task<int> GetCountAsync();
        Task<Workstation> GetByIdAsync(dynamic id);
        Task<int> GetOnlineCountAsync();
        Task<bool> ExistAsync(Expression<Func<Workstation, bool>> predicate);
        Task AddWorkstationAsync(WorkstationInfo workstationInfo);
        Task UpdateWorkstationInfoAsync(WorkstationInfo workstationInfo);
        Task EditWorkstationAsync(Workstation workstation);
        Task ApproveWorkstationAsync(Workstation workstation);
        Task UnapproveWorkstationAsync(string id);
        Task<bool> GetRfidStateAsync(string workstationId);
        Task UpdateRfidStateAsync(string workstationId);
    }
}