using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IEmployeeService _employeeService;
        private readonly IWorkstationAuditService _workstationAuditService;
        private readonly IHardwareVaultTaskService _hardwareVaultTaskService;
        private readonly IWorkstationService _workstationService;
        private readonly IHardwareVaultService _hardwareVaultService;

        public DashboardService(IEmployeeService employeeService,
                                IWorkstationAuditService workstationAuditService,
                                IHardwareVaultTaskService hardwareVaultTaskService,
                                IWorkstationService workstationService,
                                IHardwareVaultService hardwareVaultService)
        {
            _employeeService = employeeService;
            _workstationAuditService = workstationAuditService;
            _hardwareVaultTaskService = hardwareVaultTaskService;
            _workstationService = workstationService;
            _hardwareVaultService = hardwareVaultService;
        }

        #region Server

        public string GetServerVersion()
        {
            return ServerConstants.Version;
        }

        public async Task<int> GetDeviceTasksCount()
        {
            return await _hardwareVaultTaskService.TaskQuery().CountAsync();
        }

        public async Task<List<HardwareVaultTask>> GetVaultTasks()
        {
            return await _hardwareVaultTaskService.TaskQuery().ToListAsync();
        }

        public async Task<List<DashboardNotify>> GetServerNotifyAsync()
        {
            var list = new List<DashboardNotify>();
            var longPendingTasksCount = await _hardwareVaultTaskService.TaskQuery().Where(d => d.CreatedAt <= DateTime.UtcNow.AddDays(-1)).CountAsync();

            if (longPendingTasksCount > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = "Long pending tasks",
                    Count = longPendingTasksCount,
                    Page = "./DeviceTasks",
                    Handler = "LongPending"
                });
            }

            return list;
        }

        #endregion

        #region Employees

        public async Task<int> GetEmployeesCountAsync()
        {
            return await _employeeService.EmployeeQuery().CountAsync();
        }

        public async Task<int> GetEmployeesOpenedSessionsCountAsync()
        {
            return await _workstationAuditService
               .SessionQuery()
               .Where(w => w.EndDate == null)
               .CountAsync();
        }

        public async Task<List<DashboardNotify>> GetEmployeesNotifyAsync()
        {
            var list = new List<DashboardNotify>();

            var nonHideezUnlock = await _workstationAuditService
                .SessionQuery()
                .Where(w => w.StartDate >= DateTime.UtcNow.AddDays(-1) && w.UnlockedBy == Hideez.SDK.Communication.SessionSwitchSubject.NonHideez)
                .CountAsync();

            if (nonHideezUnlock > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = "Non-hideez unlock (24h)",
                    Count = nonHideezUnlock,
                    Page = "/Audit/WorkstationSessions/Index",
                    Handler = "NonHideezUnlock"
                });
            }

            var longOpenSession = await _workstationAuditService
                .SessionQuery()
                .Where(w => w.StartDate <= DateTime.UtcNow.AddHours(-12) && w.EndDate == null)
                .CountAsync();

            if (longOpenSession > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = "Long open session (>12h)",
                    Count = longOpenSession,
                    Page = "/Audit/WorkstationSessions/Index",
                    Handler = "LongOpenSession"
                });
            }

            return list;
        }

        #endregion

        #region Hardware Vaults

        public async Task<int> GetHardwareVaultsCountAsync()
        {
            return await _hardwareVaultService.VaultQuery().CountAsync();
        }

        public async Task<int> GetReadyHardwareVaultsCountAsync()
        {
            return await _hardwareVaultService.VaultQuery().Where(d => d.EmployeeId == null).CountAsync();
        }

        public async Task<List<DashboardNotify>> GetHardwareVaultsNotifyAsync()
        {
            var list = new List<DashboardNotify>();

            var lowBattery = await _hardwareVaultService
                .VaultQuery()
                .Where(d => d.Battery <= 30)
                .CountAsync();

            if (lowBattery > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = "Low battery",
                    Count = lowBattery,
                    Page = "/Devices/Index",
                    Handler = "LowBattery"
                });
            }

            var vaultLock = await _hardwareVaultService
                .VaultQuery()
                .Where(d => d.Status == VaultStatus.Deactivated)
                .CountAsync();

            if (vaultLock > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = "Vault locked",
                    Count = vaultLock,
                    Page = "/Devices/Index",
                    Handler = "DeviceLocked"
                });
            }

            var licenseWarning = await _hardwareVaultService
                .VaultQuery()
                .Where(d => d.LicenseStatus == VaultLicenseStatus.Warning)
                .AsTracking()
                .CountAsync();

            if (licenseWarning > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = "License warning",
                    Count = licenseWarning,
                    Page = "/Devices/Index",
                    Handler = "LicenseWarning"
                });
            }

            var licenseCritical = await _hardwareVaultService
                .VaultQuery()
                .Where(d => d.LicenseStatus == VaultLicenseStatus.Critical)
                .AsTracking()
                .CountAsync();

            if (licenseCritical > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = "License critical",
                    Count = licenseCritical,
                    Page = "/Devices/Index",
                    Handler = "LicenseCritical"
                });
            }

            var licenseExpired = await _hardwareVaultService
                .VaultQuery()
                .Where(d => d.LicenseStatus == VaultLicenseStatus.Expired)
                .AsTracking()
                .CountAsync();

            if (licenseExpired > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = "License expired",
                    Count = licenseExpired,
                    Page = "/Devices/Index",
                    Handler = "LicenseExpired"
                });
            }
            
            return list;
        }

        #endregion

        #region Workstations

        public async Task<int> GetWorkstationsCountAsync()
        {
            return await _workstationService.WorkstationQuery().CountAsync();
        }

        public async Task<int> GetWorkstationsOnlineCountAsync()
        {
            return await Task.FromResult(RemoteWorkstationConnectionsService.WorkstationsOnlineCount());
        }

        public async Task<List<DashboardNotify>> GetWorkstationsNotifyAsync()
        {
            var list = new List<DashboardNotify>();

            var notApproved = await _workstationService
                .WorkstationQuery()
                .Where(w => w.Approved == false)
                .CountAsync();

            if (notApproved > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = "Waiting for approval",
                    Count = notApproved,
                    Page = "/Workstations/Index",
                    Handler = "NotApproved"
                });
            }

            return list;
        }

        #endregion
    }
}