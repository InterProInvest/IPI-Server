using HES.Core.Entities;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class RemoteTaskService : IRemoteTaskService
    {
        private readonly IDeviceService _deviceService;
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly IAccountService _accountService;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly ILicenseService _licenseService;
        private readonly ILogger<RemoteTaskService> _logger;
        private readonly IHubContext<EmployeeDetailsHub> _hubContext;

        public RemoteTaskService(IDeviceService deviceService,
                                 IDeviceTaskService deviceTaskService,
                                 IAccountService deviceAccountService,
                                 IDataProtectionService dataProtectionService,
                                 ILicenseService licenseService,
                                 ILogger<RemoteTaskService> logger,
                                 IHubContext<EmployeeDetailsHub> hubContext)
        {
            _deviceService = deviceService;
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

            var device = await _deviceService.GetDeviceByIdAsync(deviceTask.DeviceId);

            var account = deviceTask.Account;

            //var properties = new List<string>() { "Status", "LastSyncedAt" };

            // Set value depending on the operation
            switch (deviceTask.Operation)
            {
                case TaskOperation.Create:
                    account.IdFromDevice = idFromDevice;
                    await _accountService.UpdateOnlyPropAsync(account, new string[] { nameof(Account.IdFromDevice) });
                    break;
                case TaskOperation.Update:
                    break;
                case TaskOperation.Delete:
                    //if (account.Employee.PrimaryAccountId == account.Id)
                    //{
                    //    account.Employee.PrimaryAccountId = null;
                    //    await _employeeService.UpdateOnlyPropAsync(account.Employee, new string[] { nameof(Employee.PrimaryAccountId) });
                    //}        
                    break;
                case TaskOperation.Primary:
                    //account.Status = AccountStatus.Done;
                    //account.LastSyncedAt = DateTime.UtcNow;
                    //await _accountService.UpdateOnlyPropAsync(account, properties.ToArray());
                    break;
                case TaskOperation.Wipe:
                    device.Status = Enums.DeviceState.OK;
                    device.MasterPassword = null;
                    await _deviceService.UpdateOnlyPropAsync(device, new string[] { nameof(Device.Status), nameof(Device.MasterPassword) });
                    await _licenseService.DiscardLicenseAppliedAsync(device.Id);
                    break;
                case TaskOperation.UnlockPin:
                    device.Status = Enums.DeviceState.OK;
                    await _deviceService.UpdateOnlyPropAsync(device, new string[] { nameof(Device.Status) });
                    break;
                case TaskOperation.Link:
                    device.MasterPassword = deviceTask.Password;
                    await _deviceService.UpdateOnlyPropAsync(device, new string[] { nameof(Device.MasterPassword) });
                    break;
                case TaskOperation.Profile:
                    break;
                default:
                    _logger.LogCritical($"[{device.Id}] unhandled case {deviceTask.Operation.ToString()}");
                    break;
            }

            // Delete task
            await _deviceTaskService.DeleteTaskAsync(deviceTask);
        }

        public async Task ExecuteRemoteTasks(string deviceId, RemoteDevice remoteDevice, TaskOperation operation)
        {
            _dataProtectionService.Validate();

            var device = await _deviceService.GetDeviceByIdAsync(deviceId);

            var query = _deviceTaskService
                .TaskQuery()
                .Include(t => t.Account)
                .Where(t => t.DeviceId == deviceId);

            // Filtering by operation
            switch (operation)
            {
                case TaskOperation.None:
                    break;
                case TaskOperation.Primary:
                    query = query.Where(t => t.AccountId == device.Employee.PrimaryAccountId || t.Operation == TaskOperation.Primary);
                    break;
                default:
                    query = query.Where(t => t.Operation == operation);
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
                        throw new HideezException(HideezErrorCode.DeviceHasBeenWiped); // further processing is not possible
                }

                tasks = await query.ToListAsync();
            }

            await _deviceService.UpdateNeedSyncAsync(device, false);

            //if (device != null)
            //{
            //    await _hubContext.Clients.All.SendAsync("UpdateTable", device.EmployeeId);
            //}
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
                    idFromDevice = await LinkDevice(remoteDevice, task);
                    break;
                case TaskOperation.Primary:
                    idFromDevice = await SetDeviceAccountAsPrimary(remoteDevice, task);
                    break;
                case TaskOperation.Profile:
                    idFromDevice = await ProfileDevice(remoteDevice, task);
                    break;
                case TaskOperation.UnlockPin:
                    idFromDevice = await UnlockPin(remoteDevice, task);
                    break;
            }
            return idFromDevice;
        }

        async Task<ushort> AddDeviceAccount(RemoteDevice remoteDevice, DeviceTask task)
        {
            var device = await _deviceService
                .DeviceQuery()
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
            var device = await _deviceService
                .DeviceQuery()
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
            var device = await _deviceService
                .DeviceQuery()
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

        async Task<ushort> LinkDevice(RemoteDevice remoteDevice, DeviceTask task)
        {
            if (!remoteDevice.AccessLevel.IsLinkRequired)
            {
                _logger.LogError($"Trying to link already linked device [{remoteDevice.Id}]");
                return 0;
            }

            var key = ConvertUtils.HexStringToBytes(task.Password);
            await remoteDevice.Link(key);
            await ProfileDevice(remoteDevice, task);
            return 0;
        }

        async Task<ushort> ProfileDevice(RemoteDevice remoteDevice, DeviceTask task)
        {
            var device = await _deviceService
                .DeviceQuery()
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

        async Task<ushort> UnlockPin(RemoteDevice remoteDevice, DeviceTask task)
        {
            var key = ConvertUtils.HexStringToBytes(task.Password);
            await remoteDevice.Unlock(key);
            return 0;
        }
    }
}