using HES.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ILicenseService
    {
        Task<List<LicenseOrder>> GetLicenseOrdersAsync();
        Task<LicenseOrder> GetLicenseOrderByIdAsync(string orderId);
        Task<LicenseOrder> CreateOrderAsync(LicenseOrder licenseOrder);
        Task DeleteOrderAsync(string orderId);
        Task SendOrderAsync(string orderId);
        Task UpdateLicenseOrdersAsync();
        Task<IList<HardwareVaultLicense>> GetNotAppliedLicensesByHardwareVaultIdAsync(string vaultId);
        Task<IList<HardwareVaultLicense>> GetLicensesByOrderIdAsync(string orderId);
        Task<List<HardwareVaultLicense>> AddHardwareVaultLicensesAsync(string orderId, List<string> vaultIds);
        Task UpdatehardwareVaultsLicenseStatusAsync();
        Task ChangeLicenseAppliedAsync(string vaultId, string licenseId);
        Task ChangeLicenseNotAppliedAsync(string vaultId);
    }
}