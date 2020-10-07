﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class RemoteTaskService : IRemoteTaskService, IDisposable
    {
        private readonly IHardwareVaultService _hardwareVaultService;
        private readonly IHardwareVaultTaskService _hardwareVaultTaskService;
        private readonly IAccountService _accountService;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly ILdapService _ldapService;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IHubContext<RefreshHub> _hubContext;

        public RemoteTaskService(IHardwareVaultService hardwareVaultService,
                                 IHardwareVaultTaskService hardwareVaultTaskService,
                                 IAccountService accountService,
                                 IDataProtectionService dataProtectionService,
                                 ILdapService ldapService,
                                 IAppSettingsService appSettingsService,
                                 IHubContext<RefreshHub> hubContext)
        {
            _hardwareVaultService = hardwareVaultService;
            _hardwareVaultTaskService = hardwareVaultTaskService;
            _accountService = accountService;
            _dataProtectionService = dataProtectionService;
            _ldapService = ldapService;
            _appSettingsService = appSettingsService;
            _hubContext = hubContext;
        }

        private async Task TaskCompleted(string taskId)
        {
            var task = await _hardwareVaultTaskService.GetTaskByIdAsync(taskId);

            if (task == null)
                throw new Exception($"Task {taskId} not found");

            switch (task.Operation)
            {
                case TaskOperation.Create:
                    await _accountService.UpdateAfterAccountCreateAsync(task.Account, task.Timestamp);
                    break;
                case TaskOperation.Update:
                case TaskOperation.Delete:
                case TaskOperation.Primary:
                    await _accountService.UpdateAfterAccountModifyAsync(task.Account, task.Timestamp);
                    break;
            }

            if (task.HardwareVaultId != null)
                await _hubContext.Clients.All.SendAsync(RefreshPage.EmployeesDetailsVaultSynced, task.HardwareVault.EmployeeId);

            // Delete task
            await _hardwareVaultTaskService.DeleteTaskAsync(task);
        }

        public async Task ExecuteRemoteTasks(string vaultId, RemoteDevice remoteDevice, bool primaryAccountOnly)
        {
            _dataProtectionService.Validate();

            var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);

            // Tasks query 
            var query = _hardwareVaultTaskService
                .TaskQuery()
                .Include(t => t.HardwareVault)
                .Include(t => t.Account)
                .Where(t => t.HardwareVaultId == vaultId);

            if (primaryAccountOnly)
                query = query.Where(x => x.AccountId == vault.Employee.PrimaryAccountId || x.Operation == TaskOperation.Primary);

            query = query.OrderBy(x => x.CreatedAt).AsNoTracking();

            var tasks = await query.ToListAsync();

            while (tasks.Any())
            {
                foreach (var task in tasks)
                {
                    task.Password = _dataProtectionService.Decrypt(task.Password);
                    task.OtpSecret = _dataProtectionService.Decrypt(task.OtpSecret);
                    await ExecuteRemoteTask(remoteDevice, task);
                    await TaskCompleted(task.Id);
                }

                tasks = await query.ToListAsync();
            }

            vault.NeedSync = false;
            await _hardwareVaultService.UpdateVaultAsync(vault);
        }

        private async Task ExecuteRemoteTask(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            switch (task.Operation)
            {
                case TaskOperation.Create:
                    if (task.Account.UpdateInActiveDirectory)
                    {
                        var ldapSettings = await _appSettingsService.GetLdapSettingsAsync();
                        if (ldapSettings?.Password == null)
                            throw new Exception("Active Directory Credentials Required"); // TODO use Communication.dll ex
                        await _ldapService.SetUserPasswordAsync(task.HardwareVault.EmployeeId, task.Password, ldapSettings);         
                    }
                    await AddAccountAsync(remoteDevice, task);
                    break;
                case TaskOperation.Update:
                    await UpdateAccountAsync(remoteDevice, task);
                    break;
                case TaskOperation.Delete:
                    await DeleteAccountAsync(remoteDevice, task);
                    break;
                case TaskOperation.Primary:
                    await SetAccountAsPrimaryAsync(remoteDevice, task);
                    break;
                case TaskOperation.Profile:
                    await ProfileVaultAsync(remoteDevice, task);
                    break;
            }
        }

        private async Task AddAccountAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            var account = await _accountService.GetAccountByIdNoTrackingAsync(task.AccountId);
            bool isPrimary = account.Employee.PrimaryAccountId == task.AccountId;

            var pm = new DevicePasswordManager(remoteDevice, null);
            await pm.SaveOrUpdateAccount(new StorageId(account.StorageId), task.Timestamp, account.Name, task.Password, account.Login, task.OtpSecret, account.Apps, account.Urls, isPrimary, new AccountFlagsOptions() { IsReadOnly = true });
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

        private async Task ProfileVaultAsync(RemoteDevice remoteDevice, HardwareVaultTask task)
        {
            var accessParams = await _hardwareVaultService.GetAccessParamsAsync(task.HardwareVaultId);
            var key = ConvertUtils.HexStringToBytes(task.Password);
            await remoteDevice.Access(DateTime.UtcNow, key, accessParams);
        }

        public async Task LinkVaultAsync(RemoteDevice remoteDevice, HardwareVault vault)
        {
            if (!remoteDevice.AccessLevel.IsLinkRequired)
                return;

            var code = Encoding.UTF8.GetBytes(await _hardwareVaultService.GetVaultActivationCodeAsync(vault.Id));
            var key = ConvertUtils.HexStringToBytes(_dataProtectionService.Decrypt(vault.MasterPassword));
            await remoteDevice.Link(key, code, 3);
            await _hardwareVaultService.SetStatusAppliedAsync(vault);
        }

        public async Task SuspendVaultAsync(RemoteDevice remoteDevice, HardwareVault vault)
        {
            if (vault.IsStatusApplied)
                return;

            var code = Encoding.UTF8.GetBytes(await _hardwareVaultService.GetVaultActivationCodeAsync(vault.Id));
            var key = ConvertUtils.HexStringToBytes(_dataProtectionService.Decrypt(vault.MasterPassword));
            await remoteDevice.LockDeviceCode(key, code, 3);
            await _hardwareVaultService.SetStatusAppliedAsync(vault);
        }

        public async Task WipeVaultAsync(RemoteDevice remoteDevice, HardwareVault vault)
        {
            if (!remoteDevice.AccessLevel.IsLinkRequired)
                await remoteDevice.Wipe(ConvertUtils.HexStringToBytes(_dataProtectionService.Decrypt(vault.MasterPassword)));

            await _hardwareVaultService.UpdateAfterWipeAsync(vault.Id);
        }

        public void Dispose()
        {
            _hardwareVaultService.Dispose();
            _hardwareVaultTaskService.Dispose();
            _accountService.Dispose();
            _ldapService.Dispose();
            _appSettingsService.Dispose();           
        }
    }
}