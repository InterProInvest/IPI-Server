using HES.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ILicenseService
    {
        IQueryable<LicenseOrder> LicenseOrderQuery();
        Task<LicenseOrder> CreateOrderAsync(LicenseOrder licenseOrder);
        Task<List<DeviceLicense>> AddDeviceLicenseAsync(string orderId, List<string> devicesIds);
        Task<List<DeviceLicense>> GetDeviceLicensesAsync();
        Task<IList<DeviceLicense>> GetNewDeviceLicensesByDeviceIdAsync(string deviceId);
        Task OnDeviceLicenseAppliedAsync(string deviceId, string licenseId);
    }
}