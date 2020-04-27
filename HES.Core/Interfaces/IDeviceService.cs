using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Models;
using HES.Core.Models.Web.HardwareVault;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceService
    {
        IQueryable<Device> VaultQuery();
        Task<Device> GetVaultByIdAsync(string id);
        Task<List<Device>> GetVaultsByEmployeeIdAsync(string id);
        Task<List<Device>> GetVaultsAsync(int skip, int take, string sortColumn, ListSortDirection sortDirection, string searchText, HardwareVaultFilter filter);
        Task<int> GetVaultsCountAsync(string searchText, HardwareVaultFilter filter);
        Task<List<Device>> GetFilteredDevicesAsync(DeviceFilter deviceFilter);
        Task<Device> AddDeviceAsync(Device device);
        Task ImportDevicesAsync();
        Task EditRfidAsync(Device device);
        Task UnchangedVaultAsync(Device vault);
        Task UpdateOnlyPropAsync(Device device, string[] properties);
        Task UpdateDeviceInfoAsync(string deviceId, int battery, string firmware, bool locked);
        Task UpdateNeedSyncAsync(Device device, bool needSync);
        Task UpdateNeedSyncAsync(IList<Device> devices, bool needSync);
        Task<bool> ExistAsync(Expression<Func<Device, bool>> predicate);
        Task RemoveEmployeeAsync(string deviceId);
        Task SetVaultStatusAsync(string vaultId, VaultStatus vaultStatus);

        Task<HardwareVaultActivation> GenerateVaultActivationAsync(string vaultId);
        Task ChangeVaultActivationStatusAsync(string vaultId, HardwareVaultActivationStatus status);
        Task<string> GetVaultActivationCodeAsync(string vaultId);
        Task ActivateVaultAsync(string vaultId);
        Task SuspendVaultAsync(string vaultId, string description);
        Task VaultCompromisedAsync(string vaultId);

        IQueryable<DeviceAccessProfile> AccessProfileQuery();
        Task<List<DeviceAccessProfile>> GetAccessProfilesAsync();
        Task<DeviceAccessProfile> GetAccessProfileByIdAsync(string id);
        Task<DeviceAccessProfile> CreateProfileAsync(DeviceAccessProfile deviceAccessProfile);
        Task EditProfileAsync(DeviceAccessProfile deviceAccessProfile);
        Task DeleteProfileAsync(string id);
        Task SetProfileAsync(string[] devicesId, string profileId);
        Task<string[]> UpdateProfileAsync(string profileId);
    }
}