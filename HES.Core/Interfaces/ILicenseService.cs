using HES.Core.Entities;
using HES.Core.Models.Web;
using HES.Core.Models.Web.LicenseOrders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ILicenseService
    {
        Task<List<LicenseOrder>> GetLicenseOrdersAsync();
        Task<List<LicenseOrder>> GetLicenseOrdersAsync(DataLoadingOptions<LicenseOrderFilter> dataLoadingOptions);
        Task<int> GetLicenseOrdersCountAsync(DataLoadingOptions<LicenseOrderFilter> dataLoadingOptions);
        Task<LicenseOrder> GetLicenseOrderByIdAsync(string orderId);
        Task<LicenseOrder> CreateOrderAsync(LicenseOrder licenseOrder, List<HardwareVault> hardwareVaults);
        Task<List<LicenseOrder>> AddOrderRangeAsync(List<LicenseOrder> licenseOrders);
        Task DeleteOrderAsync(LicenseOrder licenseOrder);
        Task SendOrderAsync(LicenseOrder licenseOrder);
        Task UpdateLicenseOrdersAsync();
        Task<List<HardwareVaultLicense>> GetLicensesAsync();
        Task<List<HardwareVaultLicense>> GetNotAppliedLicensesByHardwareVaultIdAsync(string vaultId);
        Task<List<HardwareVaultLicense>> GetLicensesByOrderIdAsync(string orderId);
        Task<List<HardwareVaultLicense>> AddHardwareVaultEmptyLicensesAsync(string orderId, List<string> vaultIds);
        Task<List<HardwareVaultLicense>> AddHardwareVaultLicenseRangeAsync(List<HardwareVaultLicense> hardwareVaultLicenses);
        Task UpdateHardwareVaultsLicenseStatusAsync();
        Task ChangeLicenseAppliedAsync(string vaultId, string licenseId);
        Task ChangeLicenseNotAppliedAsync(string vaultId);
    }
}