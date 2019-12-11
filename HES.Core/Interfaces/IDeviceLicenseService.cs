using HES.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceLicenseService
    {
        IQueryable<DeviceLicense> Query();
        Task<List<DeviceLicense>> GeDeviceLicensesAsync();
        Task<IList<DeviceLicense>> GetDeviceLicensesByDeviceIdAsync(string deviceId);
    }
}