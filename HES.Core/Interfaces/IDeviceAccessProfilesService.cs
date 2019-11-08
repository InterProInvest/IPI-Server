using HES.Core.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceAccessProfilesService
    {
        IQueryable<DeviceAccessProfile> Query();
        Task<DeviceAccessProfile> GetByIdAsync(dynamic id);
        Task CreateProfileAsync(DeviceAccessProfile deviceAccessProfile);
        Task EditProfileAsync(DeviceAccessProfile deviceAccessProfile);
        Task DeleteProfileAsync(string id);
    }
}
