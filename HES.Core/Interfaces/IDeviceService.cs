using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Models.Web;
using HES.Core.Models.Web.HardwareVault;
using Hideez.SDK.Communication.HES.DTO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IHardwareVaultService
    {
        IQueryable<HardwareVault> VaultQuery();
        Task<HardwareVault> GetVaultByIdAsync(string id);
        Task<List<HardwareVault>> GetVaultsByEmployeeIdAsync(string id);
        Task<List<HardwareVault>> GetVaultsAsync(DataLoadingOptions<HardwareVaultFilter> dataLoadingOptions);
        Task<int> GetVaultsCountAsync(DataLoadingOptions<HardwareVaultFilter> dataLoadingOptions);
        Task<HardwareVault> AddVaultIfNotExistAsync(HardwareVault vault);
        Task ImportVaultsAsync();
        Task EditRfidAsync(HardwareVault vault);
        Task UnchangedVaultAsync(HardwareVault vault);
        Task UpdateOnlyPropAsync(HardwareVault vault, string[] properties);
        Task UpdateAfterWipe(string vaultId);
        Task UpdateHardwareVaultInfoAsync(BleDeviceDto dto);
        Task UpdateNeedSyncAsync(HardwareVault vault, bool needSync);
        Task UpdateNeedSyncAsync(IList<HardwareVault> vaults, bool needSync);
        Task<HardwareVaultActivation> GenerateVaultActivationAsync(string vaultId);
        Task ChangeVaultActivationStatusAsync(string vaultId, HardwareVaultActivationStatus status);
        Task<string> GetVaultActivationCodeAsync(string vaultId);
        Task ActivateVaultAsync(string vaultId);
        Task SuspendVaultAsync(string vaultId, string description);
        Task VaultCompromisedAsync(string vaultId, VaultStatusReason reason, string description);

        IQueryable<HardwareVaultProfile> ProfileQuery();
        Task<List<HardwareVaultProfile>> GetProfilesAsync();
        Task<HardwareVaultProfile> GetProfileByIdAsync(string profileId);
        Task<List<string>> GetVaultIdsByProfileTaskAsync(string profileId);
        Task<HardwareVaultProfile> CreateProfileAsync(HardwareVaultProfile deviceAccessProfile);
        Task EditProfileAsync(HardwareVaultProfile deviceAccessProfile);
        Task DeleteProfileAsync(string id);
        Task ChangeVaultProfileAsync(string vaultId, string profileId);
    }
}