using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Models.Web;
using HES.Core.Models.Web.HardwareVaults;
using Hideez.SDK.Communication.HES.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IHardwareVaultService
    {
        IQueryable<HardwareVault> VaultQuery();
        Task<HardwareVault> GetVaultByIdAsync(string id);
        Task<HardwareVault> GetVaultByIdNoTrackingAsync(string vaultId);
        Task<List<HardwareVault>> GetVaultsWithoutLicenseAsync();
        Task<List<HardwareVault>> GetVaultsWithLicenseAsync();
        Task<List<HardwareVault>> GetVaultsAsync(DataLoadingOptions<HardwareVaultFilter> options);
        Task<int> GetVaultsCountAsync(DataLoadingOptions<HardwareVaultFilter> options);
        Task ImportVaultsAsync();
        Task<HardwareVault> UpdateVaultAsync(HardwareVault vault);
        Task<List<HardwareVault>> UpdateRangeVaultsAsync(IList<HardwareVault> vaults);
        Task UnchangedVaultAsync(HardwareVault vault);
        Task UpdateAfterWipeAsync(string vaultId);
        Task UpdateAfterLinkAsync(string vaultId, string masterPassword);
        Task UpdateHardwareVaultInfoAsync(BleDeviceDto dto);
        Task<HardwareVaultActivation> GenerateVaultActivationAsync(string vaultId);
        Task ChangeVaultActivationStatusAsync(string vaultId, HardwareVaultActivationStatus status);
        Task<string> GetVaultActivationCodeAsync(string vaultId);
        Task ActivateVaultAsync(string vaultId);
        Task SuspendVaultAsync(string vaultId, string description);
        Task VaultCompromisedAsync(string vaultId, VaultStatusReason reason, string description);

        IQueryable<HardwareVaultProfile> ProfileQuery();
        Task<List<HardwareVaultProfile>> GetProfilesAsync();
        Task<HardwareVaultProfile> GetProfileByIdAsync(string profileId);
        Task<List<string>> GetVaultIdsByProfileTaskAsync();
        Task<HardwareVaultProfile> CreateProfileAsync(HardwareVaultProfile hardwareVaultProfile);
        Task EditProfileAsync(HardwareVaultProfile hardwareVaultProfile);
        Task DeleteProfileAsync(string id);
        Task ChangeVaultProfileAsync(string vaultId, string profileId);
    }
}