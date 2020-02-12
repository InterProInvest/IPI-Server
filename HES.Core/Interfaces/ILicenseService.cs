using HES.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ILicenseService
    {
        Task<List<LicenseOrder>> GetLicenseOrdersAsync();
        Task<LicenseOrder> GetLicenseOrderByIdAsync(string id);
        Task<LicenseOrder> CreateOrderAsync(LicenseOrder licenseOrder);
        Task DeleteOrderAsync(string id);
        Task SendOrderAsync(string orderId);
        Task UpdateLicenseOrdersAsync();
        Task<IList<DeviceLicense>> GetDeviceLicensesByDeviceIdAsync(string deviceId);
        Task<IList<DeviceLicense>> GetDeviceLicensesByOrderIdAsync(string orderId);
        Task<List<DeviceLicense>> AddDeviceLicensesAsync(string orderId, List<string> devicesIds);
        Task UpdateDeviceLicenseStatusAsync();
        Task SetDeviceLicenseAppliedAsync(string deviceId, string licenseId);
        Task DiscardLicenseAppliedAsync(string deviceId);
    }
}