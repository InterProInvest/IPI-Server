using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class RemoteTaskService : IRemoteTaskService
    {
        private readonly IHardwareVaultService _hardwareVaultService;
        private readonly IHardwareVaultTaskService _deviceTaskService;
        private readonly IAccountService _accountService;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly ILicenseService _licenseService;
        private readonly ILogger<RemoteTaskService> _logger;
        private readonly IHubContext<EmployeeDetailsHub> _hubContext;

        public RemoteTaskService(IHardwareVaultService hardwareVaultService,
                                 IHardwareVaultTaskService deviceTaskService,
                                 IAccountService deviceAccountService,
                                 IDataProtectionService dataProtectionService,
                                 ILicenseService licenseService,
                                 ILogger<RemoteTaskService> logger,
                                 IHubContext<EmployeeDetailsHub> hubContext)
        {
            _hardwareVaultService = hardwareVaultService;
            _deviceTaskService = deviceTaskService;
            _accountService = deviceAccountService;
            _dataProtectionService = dataProtectionService;
            _licenseService = licenseService;
            _logger = logger;
            _hubContext = hubContext;
        }

        async Task TaskCompleted(string taskId, ushort idFromDevice)
        {
            var deviceTask = await _deviceTaskService.GetTaskByIdAsync(taskId);

            if (deviceTask == null)
                throw new Exception($"Device Task {taskId} not found");

            var device = await _hardwareVaultService.GetVaultByIdAsync(deviceTask.HardwareVaultId);

            var account = deviceTask.Account;

            switch (deviceTask.Operation)
            {
                case TaskOperation.Create:
                    account.IdFromDevice = idFromDevice;
                    await _accountService.UpdateOnlyPropAsync(account, new string[] { nameof(Account.IdFromDevice) });
                    break;
                case TaskOperation.Update:
                case TaskOperation.Delete:
                case TaskOperation.Primary:
                case TaskOperation.Profile:
                case TaskOperation.Suspend:
                    break;
                case TaskOperation.Wipe:
                    device.Status = Enums.VaultStatus.Ready;
                    device.MasterPassword = null;
                    await _hardwareVaultService.UpdateOnlyPropAsync(device, new string[] { nameof(HardwareVault.Status), nameof(HardwareVault.MasterPassword) });
                    await _licenseService.DiscardLicenseAppliedAsync(device.Id);
                    break;
                case TaskOperation.Link:
                    device.MasterPassword = deviceTask.Password;
                    await _hardwareVaultService.UpdateOnlyPropAsync(device, new string[] { nameof(HardwareVault.MasterPassword) });
                    break;
                default:
                    _logger.LogCritical($"Unhandled task operation ({deviceTask.Operation})");
                    break;
            }

            // Delete task
            await _deviceTaskService.DeleteTaskAsync(deviceTask);
        }

        public async Task ExecuteRemoteTasks(string vaultId, RemoteDevice remoteDevice, TaskOperation operation)
        {
            _dataProtectionService.Validate();

            var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);

            // Execute CRUD tasks only if status Active 
            if (vault.Status != VaultStatus.Active && (operation == TaskOperation.Create || operation == TaskOperation.Update || operation == TaskOperation.Delete))
                return;

            // Crete Tasks query 
            var query = _deviceTaskService
                .TaskQuery()
                .Include(t => t.Account)
                .Where(t => t.HardwareVaultId == vaultId);

            switch (operation)
            {
                // For all task operations
                case TaskOperation.None:
                    break;
                // For setting primary account
                case TaskOperation.Primary:
                    query = query.Where(x => x.AccountId == vault.Employee.PrimaryAccountId || x.Operation == TaskOperation.Primary);
                    break;
                // For current task operation
                default:
                    query = query.Where(x => x.Operation == operation);
                    break;
            }

            query = query.OrderBy(x => x.CreatedAt).AsNoTracking();

            var tasks = await query.ToListAsync();

            while (tasks.Any())
            {
                foreach (var task in tasks)
                {
                    task.Password = _dataProtectionService.Decrypt(task.Password);
                    task.OtpSecret = _dataProtectionService.Decrypt(task.OtpSecret);
                    var idFromDevice = await ExecuteRemoteTask(remoteDevice, task);
                    await TaskCompleted(task.Id, idFromDevice);

                    if (task.Operation == TaskOperation.Wipe)
                        throw new HideezException(HideezErrorCode.DeviceHasBeenWiped); // Further processing is not possible
                }

                tasks = await query.ToListAsync();
            }

            await _hardwareVaultService.UpdateNeedSyncAsync(vault, false);
        }

        async Task<ushort> ExecuteRemoteTask(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            ushort idFromDevice = 0;
            switch (task.Operation)
            {
                case TaskOperation.Create:
                    idFromDevice = await AddAccountAsync(remoteDevice, task);
                    break;
                case TaskOperation.Update:
                    idFromDevice = await UpdateAccountAsync(remoteDevice, task);
                    break;
                case TaskOperation.Delete:
                    idFromDevice = await DeleteAccountAsync(remoteDevice, task);
                    break;
                case TaskOperation.Wipe:
                    idFromDevice = await WipeVaultAsync(remoteDevice, task);
                    break;
                case TaskOperation.Link:
                    idFromDevice = await LinkVaultAsync(remoteDevice, task);
                    break;
                case TaskOperation.Primary:
                    idFromDevice = await SetAccountAsPrimaryAsync(remoteDevice, task);
                    break;
                case TaskOperation.Profile:
                    idFromDevice = await ProfileVaultAsync(remoteDevice, task);
                    break;
                case TaskOperation.Suspend:
                    idFromDevice = await SuspendVaultAsync(remoteDevice, task);
                    break;
            }
            return idFromDevice;
        }

        private async Task<ushort> AddAccountAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            var device = await _hardwareVaultService
                .VaultQuery()
                .Include(x => x.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == task.HardwareVaultId);

            var account = await _accountService
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == task.AccountId);

            bool isPrimary = device.Employee.PrimaryAccountId == task.AccountId;

            var pm = new DevicePasswordManager(remoteDevice, null);

            ushort key = 0;
            key = await pm.SaveOrUpdateAccount(key, account.Name, task.Password, account.Login, task.OtpSecret, account.Apps, account.Urls, isPrimary, new AccountFlagsOptions() { IsReadOnly = true });

            return key;
        }

        private async Task<ushort> UpdateAccountAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            var device = await _hardwareVaultService
                .VaultQuery()
                .Include(x => x.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == task.HardwareVaultId);

            var deviceAccount = await _accountService
               .Query()
               .AsNoTracking()
               .FirstOrDefaultAsync(d => d.Id == task.AccountId);

            bool isPrimary = device.Employee.PrimaryAccountId == task.AccountId;

            var pm = new DevicePasswordManager(remoteDevice, null);

            var account = await _accountService.Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == task.AccountId);
            ushort key = account.IdFromDevice;
            key = await pm.SaveOrUpdateAccount(key, deviceAccount.Name, task.Password, deviceAccount.Login, task.OtpSecret, deviceAccount.Apps, deviceAccount.Urls, isPrimary, new AccountFlagsOptions() { IsReadOnly = true });

            return key;
        }

        private async Task<ushort> SetAccountAsPrimaryAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            var pm = new DevicePasswordManager(remoteDevice, null);

            var account = await _accountService.Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == task.AccountId);
            ushort key = account.IdFromDevice;
            key = await pm.SaveOrUpdateAccount(key, null, null, null, null, null, null, true, new AccountFlagsOptions() { IsReadOnly = true });

            return key;
        }

        private async Task<ushort> DeleteAccountAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            var device = await _hardwareVaultService
                .VaultQuery()
                .Include(x => x.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == task.HardwareVaultId);

            bool isPrimary = device.Employee.PrimaryAccountId == task.AccountId;
            var pm = new DevicePasswordManager(remoteDevice, null);
            var account = await _accountService.Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == task.AccountId);
            ushort key = account.IdFromDevice;
            await pm.DeleteAccount(key, isPrimary);
            return 0;
        }

        private async Task<ushort> WipeVaultAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            if (remoteDevice.AccessLevel.IsLinkRequired == true)
            {
                _logger.LogError($"Trying to wipe the empty device [{remoteDevice.Id}]");
                return 0;
            }

            var key = ConvertUtils.HexStringToBytes(task.Password);
            await remoteDevice.Wipe(key);
            return 0;
        }

        private async Task<ushort> LinkVaultAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            if (!remoteDevice.AccessLevel.IsLinkRequired)
            {
                _logger.LogError($"Trying to link already linked vault [{remoteDevice.Id}]");
                return 0;
            }

            var code = Encoding.UTF8.GetBytes(await _hardwareVaultService.GetVaultActivationCodeAsync(task.HardwareVaultId));
            var key = ConvertUtils.HexStringToBytes(task.Password);

            await remoteDevice.Link(key, code, 3);
            await ProfileVaultAsync(remoteDevice, task);
            return 0;
        }

        private async Task<ushort> ProfileVaultAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            var device = await _hardwareVaultService
                .VaultQuery()
                .Include(d => d.HardwareVaultProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == task.HardwareVaultId);

            var accessParams = new AccessParams()
            {
                MasterKey_Bond = device.HardwareVaultProfile.MasterKeyBonding,
                MasterKey_Connect = device.HardwareVaultProfile.MasterKeyConnection,
                MasterKey_Channel = device.HardwareVaultProfile.MasterKeyNewChannel,

                Button_Bond = device.HardwareVaultProfile.ButtonBonding,
                Button_Connect = device.HardwareVaultProfile.ButtonConnection,
                Button_Channel = device.HardwareVaultProfile.ButtonNewChannel,

                Pin_Bond = device.HardwareVaultProfile.PinBonding,
                Pin_Connect = device.HardwareVaultProfile.PinConnection,
                Pin_Channel = device.HardwareVaultProfile.PinNewChannel,

                PinMinLength = device.HardwareVaultProfile.PinLength,
                PinMaxTries = device.HardwareVaultProfile.PinTryCount,
                PinExpirationPeriod = device.HardwareVaultProfile.PinExpiration,
                ButtonExpirationPeriod = 0,
                MasterKeyExpirationPeriod = 0
            };

            var key = ConvertUtils.HexStringToBytes(task.Password);
            await remoteDevice.Access(DateTime.UtcNow, key, accessParams);
            return 0;
        }

        private async Task<ushort> SuspendVaultAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            var code = Encoding.UTF8.GetBytes(await _hardwareVaultService.GetVaultActivationCodeAsync(task.HardwareVaultId));
            var vault = await _hardwareVaultService.GetVaultByIdAsync(task.HardwareVaultId);
            var key = ConvertUtils.HexStringToBytes(vault.MasterPassword);
            await remoteDevice.LockDeviceCode(key, code, 3);
            return 0;
        }
    }
}
