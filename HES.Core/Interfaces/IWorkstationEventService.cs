using HES.Core.Entities;
using Hideez.SDK.Communication.HES.DTO;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationEventService
    {
        IQueryable<WorkstationEvent> Query();
        Task AddEventAsync(WorkstationEvent workstationEvent);
        Task AddEventDtoAsync(WorkstationEventDto workstationEventDto);
    }
}