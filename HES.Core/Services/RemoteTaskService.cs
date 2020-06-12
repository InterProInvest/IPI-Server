using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
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
        private readonly IHardwareVaultTaskService _hardwareVaultTaskService;
        private readonly IAccountService _accountService;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly ILogger<RemoteTaskService> _logger;
        private readonly IHubContext<EmployeeDetailsHub> _hubContext;

        public RemoteTaskService(IHardwareVaultService hardwareVaultService,
                                 IHardwareVaultTaskService hardwareVaultTaskService,
                                 IAccountService accountService,
                                 IDataProtectionService dataProtectionService,
                                 ILogger<RemoteTaskService> logger,
                                 IHubContext<EmployeeDetailsHub> hubContext)
        {
            _hardwareVaultService = hardwareVaultService;
            _hardwareVaultTaskService = hardwareVaultTaskService;
            _accountService = accountService;
            _dataProtectionService = dataProtectionService;
            _logger = logger;
            _hubContext = hubContext;
        }

        private async Task TaskCompleted(string taskId, byte[] storageId)
        {
            var task = await _hardwareVaultTaskService.GetTaskByIdAsync(taskId);

            if (task == null)
                throw new Exception($"Task {taskId} not found");

            switch (task.Operation)
            {
                case TaskOperation.Create:
                    await _accountService.UpdateAfterAccountCreateAsync(task.Account, storageId, task.Timestamp);
                    break;
                case TaskOperation.Update:
                case TaskOperation.Delete:
                case TaskOperation.Primary:
                    await _accountService.UpdateAfterAccountModifyAsync(task.Account, task.Timestamp);
                    break;
                case TaskOperation.Profile:
                case TaskOperation.Suspend:
                    break;
                case TaskOperation.Wipe:
                    await _hardwareVaultService.UpdateAfterWipeAsync(task.HardwareVaultId);
                    break;
                case TaskOperation.Link:
                    await _hardwareVaultService.UpdateAfterLinkAsync(task.HardwareVaultId, task.Password);
                    break;
                default:
                    _logger.LogCritical($"Unhandled task operation ({task.Operation})");
                    break;
            }

            if (task.HardwareVaultId != null)
            {                
                await _hubContext.Clients.All.SendAsync("UpdatePage", task.HardwareVault.EmployeeId, string.Empty);
            }
            // Delete task
            await _hardwareVaultTaskService.DeleteTaskAsync(task);
        }

        public async Task ExecuteRemoteTasks(string vaultId, RemoteDevice remoteDevice, TaskOperation operation)
        {
            _dataProtectionService.Validate();

            var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);

            // Execute CRUD tasks only if status Active 
            if (vault.Status != VaultStatus.Active && (operation == TaskOperation.Create || operation == TaskOperation.Update || operation == TaskOperation.Delete))
                return;

            // Crete Tasks query 
            var query = _hardwareVaultTaskService
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
                    var storageId = await ExecuteRemoteTask(remoteDevice, task);
                    await TaskCompleted(task.Id, storageId);

                    if (task.Operation == TaskOperation.Wipe)
                        throw new HideezException(HideezErrorCode.DeviceHasBeenWiped); // Further processing is not possible
                }

                tasks = await query.ToListAsync();
            }

            vault.NeedSync = false;
            await _hardwareVaultService.UpdateVaultAsync(vault);
        }

        private async Task<byte[]> ExecuteRemoteTask(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            byte[] storageId = null;
            switch (task.Operation)
            {
                case TaskOperation.Create:
                    storageId = await AddAccountAsync(remoteDevice, task);
                    break;
                case TaskOperation.Update:
                    await UpdateAccountAsync(remoteDevice, task);
                    break;
                case TaskOperation.Delete:
                    await DeleteAccountAsync(remoteDevice, task);
                    break;
                case TaskOperation.Wipe:
                    await WipeVaultAsync(remoteDevice, task);
                    break;
                case TaskOperation.Link:
                    await LinkVaultAsync(remoteDevice, task);
                    break;
                case TaskOperation.Primary:
                    await SetAccountAsPrimaryAsync(remoteDevice, task);
                    break;
                case TaskOperation.Profile:
                    await ProfileVaultAsync(remoteDevice, task);
                    break;
                case TaskOperation.Suspend:
                    await SuspendVaultAsync(remoteDevice, task);
                    break;
            }
            return storageId;
        }

        private async Task<byte[]> AddAccountAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            var account = await _accountService.GetAccountByIdNoTrackingAsync(task.AccountId);
            bool isPrimary = account.Employee.PrimaryAccountId == task.AccountId;

            var storageId = new StorageId();
            var pm = new DevicePasswordManager(remoteDevice, null);
            await pm.SaveOrUpdateAccount(storageId, task.Timestamp, account.Name, task.Password, account.Login, task.OtpSecret, account.Apps, account.Urls, isPrimary, new AccountFlagsOptions() { IsReadOnly = true });

            return storageId.Data;
        }

        private async Task UpdateAccountAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            var account = await _accountService.GetAccountByIdNoTrackingAsync(task.AccountId);
            bool isPrimary = account.Employee.PrimaryAccountId == task.AccountId;

            var storageId = new StorageId(account.StorageId);
            var pm = new DevicePasswordManager(remoteDevice, null);
            await pm.SaveOrUpdateAccount(storageId, task.Timestamp, account.Name, task.Password, account.Login, task.OtpSecret, account.Apps, account.Urls, isPrimary, new AccountFlagsOptions() { IsReadOnly = true });
        }

        private async Task SetAccountAsPrimaryAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            var account = await _accountService.GetAccountByIdNoTrackingAsync(task.AccountId);

            var storageId = new StorageId(account.StorageId);
            var pm = new DevicePasswordManager(remoteDevice, null);
            await pm.SaveOrUpdateAccount(storageId, task.Timestamp, null, null, null, null, null, null, true, new AccountFlagsOptions() { IsReadOnly = true });
        }

        private async Task DeleteAccountAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            var account = await _accountService.GetAccountByIdNoTrackingAsync(task.AccountId);
            bool isPrimary = account.Employee.PrimaryAccountId == task.AccountId;

            var storageId = new StorageId(account.StorageId);
            var pm = new DevicePasswordManager(remoteDevice, null);
            await pm.DeleteAccount(storageId, isPrimary);
        }

        private async Task WipeVaultAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            if (remoteDevice.AccessLevel.IsLinkRequired == true)
            {
                _logger.LogError($"Trying to wipe the empty hardware vault [{remoteDevice.Id}]");
                return;
            }

            var key = ConvertUtils.HexStringToBytes(task.Password);
            await remoteDevice.Wipe(key);
        }

        private async Task LinkVaultAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            if (!remoteDevice.AccessLevel.IsLinkRequired)
            {
                _logger.LogError($"Trying to link already linked hardware vault [{remoteDevice.Id}]");
                return;
            }

            var code = Encoding.UTF8.GetBytes(await _hardwareVaultService.GetVaultActivationCodeAsync(task.HardwareVaultId));
            var key = ConvertUtils.HexStringToBytes(task.Password);

            await remoteDevice.Link(key, code, 3);
        }

        private async Task ProfileVaultAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            var accessParams = await _hardwareVaultService.GetAccessParamsAsync(task.HardwareVaultId);
            var key = ConvertUtils.HexStringToBytes(task.Password);
            await remoteDevice.Access(DateTime.UtcNow, key, accessParams);
        }

        private async Task SuspendVaultAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            var code = Encoding.UTF8.GetBytes(await _hardwareVaultService.GetVaultActivationCodeAsync(task.HardwareVaultId));
            var vault = await _hardwareVaultService.GetVaultByIdAsync(task.HardwareVaultId);
            var key = ConvertUtils.HexStringToBytes(_dataProtectionService.Decrypt(vault.MasterPassword));
            await remoteDevice.LockDeviceCode(key, code, 3);
        }
    }
}