using HES.Core.Entities;
using HES.Core.Models;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IEmployeeService _employeeService;
        private readonly IWorkstationAuditService _workstationAuditService;
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly IWorkstationService _workstationService;
        private readonly IDeviceService _deviceService;
        private readonly IAppService _appService;

        public DashboardService(IEmployeeService employeeService,
                                IWorkstationAuditService workstationAuditService,
                                IDeviceTaskService deviceTaskService,
                                IWorkstationService workstationService,
                                IDeviceService deviceService,
                                IAppService appService)
        {
            _employeeService = employeeService;
            _workstationAuditService = workstationAuditService;
            _deviceTaskService = deviceTaskService;
            _workstationService = workstationService;
            _deviceService = deviceService;
            _appService = appService;
        }

        #region Server

        public string GetServerVersion()
        {
            return _appService.Version;
        }

        public async Task<int> GetDeviceTasksCount()
        {
            return await _deviceTaskService.GetCountAsync();
        }

        public async Task<List<DeviceTask>> GetDeviceTasks()
        {
            return await _deviceTaskService.Query().ToListAsync();
        }

        public async Task<List<DashboardNotify>> GetServerNotify()
        {
            var list = new List<DashboardNotify>();
            var longPendingTasksCount = await _deviceTaskService.Query().Where(d => d.CreatedAt <= DateTime.UtcNow.AddDays(-1)).CountAsync();

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

        public async Task<int> GetEmployeesCount()
        {
            return await _employeeService.Query().CountAsync();
        }

        public async Task<int> GetEmployeesOpenedSessionsCount()
        {
            return await _workstationAuditService.GetOpenedSessionsCountAsync();
        }

        public async Task<List<DashboardNotify>> GetEmployeesNotify()
        {
            var list = new List<DashboardNotify>();

            var nonHideezUnlock = await _workstationAuditService
                .QueryOfSession()
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
                .QueryOfSession()
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

        #region Devices

        public async Task<int> GetDevicesCount()
        {
            return await _deviceService.GetCountAsync();
        }

        public async Task<int> GetFreeDevicesCount()
        {
            return await _deviceService.GetFreeDevicesCount();
        }

        public async Task<List<DashboardNotify>> GetDevicesNotify()
        {
            var list = new List<DashboardNotify>();

            var lowBattery = await _deviceService
                .Query()
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

            var deviceLock = await _deviceService
                .Query()
                .Where(d => d.State == DeviceState.Locked)
                .CountAsync();

            if (deviceLock > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = "Device lock",
                    Count = deviceLock,
                    Page = "/Devices/Index",
                    Handler = "DeviceLocked"
                });
            }

            var deviceError = await _deviceService
               .Query()
               .Where(d => d.State == DeviceState.Error)
               .CountAsync();

            if (deviceError > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = "Device error",
                    Count = deviceError,
                    Page = "/Devices/Index",
                    Handler = "DeviceError"
                });
            }

            return list;
        }

        #endregion

        #region Workstations

        public async Task<int> GetWorkstationsCount()
        {
            return await _workstationService.QueryOfWorkstation().CountAsync();
        }

        public async Task<int> GetWorkstationsOnlineCount()
        {
            return await _workstationService.GetOnlineCountAsync();
        }

        public async Task<List<DashboardNotify>> GetWorkstationsNotify()
        {
            var list = new List<DashboardNotify>();

            var notApproved = await _workstationService
                .QueryOfWorkstation()
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
