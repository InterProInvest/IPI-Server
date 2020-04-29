using HES.Core.Interfaces;
using HES.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<IndexModel> _logger;

        public string ServerVersion { get; set; }
        public int ServerRemoteTasksCount { get; set; }
        public IList<DashboardNotify> ServerNotify { get; set; }

        public int RegisteredEmployeesCount { get; set; }
        public int EmployeesSessionsCount { get; set; }
        public IList<DashboardNotify> EmployeesNotify { get; set; }

        public int RegisteredDevicesCount { get; set; }
        public int FreeDevicesCount { get; set; }
        public IList<DashboardNotify> DevicesNotify { get; set; }

        public int RegisteredWorkstationsCount { get; set; }
        public int WorkstationsOnlineCount { get; set; }
        public IList<DashboardNotify> WorkstationsNotify { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(IDashboardService dashboardService, ILogger<IndexModel> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                await GetServerInfoAsync();
                await GetEmployeesInfoAsync();
                await GetDevicesInfoAsync();
                await GetWorkstationsInfoAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }
        }

        private async Task GetServerInfoAsync()
        {
            ServerVersion = _dashboardService.GetServerVersion();
            ServerRemoteTasksCount = await _dashboardService.GetDeviceTasksCount();

            ServerNotify = await _dashboardService.GetServerNotifyAsync();
        }

        private async Task GetEmployeesInfoAsync()
        {
            RegisteredEmployeesCount = await _dashboardService.GetEmployeesCountAsync();
            EmployeesSessionsCount = await _dashboardService.GetEmployeesOpenedSessionsCountAsync();

            EmployeesNotify = await _dashboardService.GetEmployeesNotifyAsync();
        }

        private async Task GetDevicesInfoAsync()
        {
            RegisteredDevicesCount = await _dashboardService.GetHardwareVaultsCountAsync();
            FreeDevicesCount = await _dashboardService.GetReadyHardwareVaultsCountAsync();

            DevicesNotify = await _dashboardService.GetHardwareVaultsNotifyAsync();
        }

        private async Task GetWorkstationsInfoAsync()
        {
            RegisteredWorkstationsCount = await _dashboardService.GetWorkstationsCountAsync();
            WorkstationsOnlineCount = await _dashboardService.GetWorkstationsOnlineCountAsync();

            WorkstationsNotify = await _dashboardService.GetWorkstationsNotifyAsync();
        }
    }
}