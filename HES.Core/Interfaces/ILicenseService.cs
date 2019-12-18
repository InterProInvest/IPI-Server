using HES.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ILicenseService
    {
        IQueryable<LicenseOrder> LicenseOrderQuery();
        Task<List<LicenseOrder>> GetLicenseOrdersAsync();
        Task<LicenseOrder> GetLicenseOrderByIdAsync(string id);
        Task<LicenseOrder> CreateOrderAsync(LicenseOrder licenseOrder);
        Task DeleteOrderAsync(string id);
        Task SetStatusSent(LicenseOrder licenseOrder);
        Task<List<DeviceLicense>> AddDeviceLicensesAsync(string orderId, List<string> devicesIds);
        Task<IList<DeviceLicense>> GetDeviceLicensesByDeviceIdAsync(string deviceId);
        Task<IList<DeviceLicense>> GetDeviceLicensesByOrderIdAsync(string orderId);
        Task SetDeviceLicenseAppliedAsync(string deviceId, string licenseId);
    }
}