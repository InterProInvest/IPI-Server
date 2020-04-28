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
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class RemoteTaskService : IRemoteTaskService
    {
        private readonly IDeviceService _hardwareVaultService;
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly IAccountService _accountService;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly ILicenseService _licenseService;
        private readonly ILogger<RemoteTaskService> _logger;
        private readonly IHubContext<EmployeeDetailsHub> _hubContext;

        public RemoteTaskService(IDeviceService hardwareVaultService,
                                 IDeviceTaskService deviceTaskService,
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

            var device = await _hardwareVaultService.GetVaultByIdAsync(deviceTask.DeviceId);

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
                    await _hardwareVaultService.UpdateOnlyPropAsync(device, new string[] { nameof(Device.Status), nameof(Device.MasterPassword) });
                    await _licenseService.DiscardLicenseAppliedAsync(device.Id);
                    break;
                case TaskOperation.Link:
                    device.MasterPassword = deviceTask.Password;
                    await _hardwareVaultService.UpdateOnlyPropAsync(device, new string[] { nameof(Device.MasterPassword) });
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
                .Where(t => t.DeviceId == vaultId);

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

        async Task<ushort> ExecuteRemoteTask(RemoteDevice remoteDevice, DeviceTask task)
        {
            ushort idFromDevice = 0;
            switch (task.Operation)
            {
                case TaskOperation.Create:
                    idFromDevice = await AddDeviceAccount(remoteDevice, task);
                    break;
                case TaskOperation.Update:
                    idFromDevice = await UpdateDeviceAccount(remoteDevice, task);
                    break;
                case TaskOperation.Delete:
                    idFromDevice = await DeleteDeviceAccount(remoteDevice, task);
                    break;
                case TaskOperation.Wipe:
                    idFromDevice = await WipeDevice(remoteDevice, task);
                    break;
                case TaskOperation.Link:
                    idFromDevice = await LinkVaultAsync(remoteDevice, task);
                    break;
                case TaskOperation.Primary:
                    idFromDevice = await SetDeviceAccountAsPrimary(remoteDevice, task);
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

        async Task<ushort> AddDeviceAccount(RemoteDevice remoteDevice, DeviceTask task)
        {
            var device = await _hardwareVaultService
                .VaultQuery()
                .Include(x => x.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == task.DeviceId);

            var account = await _accountService
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == task.AccountId);

            bool isPrimary = device.Employee.PrimaryAccountId == task.AccountId;

            var pm = new DevicePasswordManager(remoteDevice, null);

            ushort key = task.Account.IdFromDevice;
            key = await pm.SaveOrUpdateAccount(key, account.Name, task.Password, account.Login, task.OtpSecret, account.Apps, account.Urls, isPrimary, new AccountFlagsOptions() { IsReadOnly = true });

            return key;
        }

        async Task<ushort> UpdateDeviceAccount(RemoteDevice remoteDevice, DeviceTask task)
        {
            var device = await _hardwareVaultService
                .VaultQuery()
                .Include(x => x.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == task.DeviceId);

            var deviceAccount = await _accountService
               .Query()
               .AsNoTracking()
               .FirstOrDefaultAsync(d => d.Id == task.AccountId);

            bool isPrimary = device.Employee.PrimaryAccountId == task.AccountId;

            var pm = new DevicePasswordManager(remoteDevice, null);

            ushort key = task.Account.IdFromDevice;
            key = await pm.SaveOrUpdateAccount(key, deviceAccount.Name, task.Password, deviceAccount.Login, task.OtpSecret, deviceAccount.Apps, deviceAccount.Urls, isPrimary, new AccountFlagsOptions() { IsReadOnly = true });

            return key;
        }

        async Task<ushort> SetDeviceAccountAsPrimary(RemoteDevice remoteDevice, DeviceTask task)
        {
            var pm = new DevicePasswordManager(remoteDevice, null);

            ushort key = task.Account.IdFromDevice;
            key = await pm.SaveOrUpdateAccount(key, null, null, null, null, null, null, true, new AccountFlagsOptions() { IsReadOnly = true });

            return key;
        }

        async Task<ushort> DeleteDeviceAccount(RemoteDevice remoteDevice, DeviceTask task)
        {
            var device = await _hardwareVaultService
                .VaultQuery()
                .Include(x => x.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == task.DeviceId);

            bool isPrimary = device.Employee.PrimaryAccountId == task.AccountId;
            var pm = new DevicePasswordManager(remoteDevice, null);
            ushort key = task.Account.IdFromDevice;
            await pm.DeleteAccount(key, isPrimary);
            return 0;
        }

        async Task<ushort> WipeDevice(RemoteDevice remoteDevice, DeviceTask task)
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

        async Task<ushort> LinkVaultAsync(RemoteDevice remoteDevice, DeviceTask task)
        {
            if (!remoteDevice.AccessLevel.IsLinkRequired)
            {
                _logger.LogError($"Trying to link already linked device [{remoteDevice.Id}]");
                return 0;
            }
            //TODOSTATUS
            var code = await _hardwareVaultService.GetVaultActivationCodeAsync(task.DeviceId);
            var key = ConvertUtils.HexStringToBytes(task.Password);
            //await remoteDevice.Link(key, code);
            await remoteDevice.Link(key);
            await ProfileVaultAsync(remoteDevice, task);
            return 0;
        }

        async Task<ushort> ProfileVaultAsync(RemoteDevice remoteDevice, DeviceTask task)
        {
            var device = await _hardwareVaultService
                .VaultQuery()
                .Include(d => d.DeviceAccessProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == task.DeviceId);

            var accessParams = new AccessParams()
            {
                MasterKey_Bond = device.DeviceAccessProfile.MasterKeyBonding,
                MasterKey_Connect = device.DeviceAccessProfile.MasterKeyConnection,
                MasterKey_Channel = device.DeviceAccessProfile.MasterKeyNewChannel,

                Button_Bond = device.DeviceAccessProfile.ButtonBonding,
                Button_Connect = device.DeviceAccessProfile.ButtonConnection,
                Button_Channel = device.DeviceAccessProfile.ButtonNewChannel,

                Pin_Bond = device.DeviceAccessProfile.PinBonding,
                Pin_Connect = device.DeviceAccessProfile.PinConnection,
                Pin_Channel = device.DeviceAccessProfile.PinNewChannel,

                PinMinLength = device.DeviceAccessProfile.PinLength,
                PinMaxTries = device.DeviceAccessProfile.PinTryCount,
                PinExpirationPeriod = device.DeviceAccessProfile.PinExpiration,
                ButtonExpirationPeriod = 0,
                MasterKeyExpirationPeriod = 0
            };

            var key = ConvertUtils.HexStringToBytes(task.Password);
            await remoteDevice.Access(DateTime.UtcNow, key, accessParams);
            return 0;
        }

        async Task<ushort> SuspendVaultAsync(RemoteDevice remoteDevice, DeviceTask task)
        {
            //TODOSTATUS
            var code = await _hardwareVaultService.GetVaultActivationCodeAsync(task.DeviceId);
            var vault = await _hardwareVaultService.GetVaultByIdAsync(task.DeviceId);

            var key = ConvertUtils.HexStringToBytes(vault.MasterPassword);
            //await remoteDevice.Unlock(key);
            return 0;
        }
    }
}