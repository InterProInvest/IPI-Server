using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Models;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

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

        public async Task OnGet()
        {
            try
            {
                await GetServerInfo();
                await GetEmployeesInfo();
                await GetDevicesInfo();
                await GetWorkstationsInfo();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }
        }

        private async Task GetServerInfo()
        {
            ServerVersion = _dashboardService.GetServerVersion();
            ServerRemoteTasksCount = await _dashboardService.GetDeviceTasksCount();

            ServerNotify = await _dashboardService.GetServerNotify();
        }

        private async Task GetEmployeesInfo()
        {
            RegisteredEmployeesCount = await _dashboardService.GetEmployeesCount();
            EmployeesSessionsCount = await _dashboardService.GetEmployeesOpenedSessionsCount();

            EmployeesNotify = await _dashboardService.GetEmployeesNotify();
        }

        private async Task GetDevicesInfo()
        {
            RegisteredDevicesCount = await _dashboardService.GetDevicesCount();
            FreeDevicesCount = await _dashboardService.GetFreeDevicesCount();

            DevicesNotify = await _dashboardService.GetDevicesNotify();
        }

        private async Task GetWorkstationsInfo()
        {
            RegisteredWorkstationsCount = await _dashboardService.GetWorkstationsCount();
            WorkstationsOnlineCount = await _dashboardService.GetWorkstationsOnlineCount();

            WorkstationsNotify = await _dashboardService.GetWorkstationsNotify();
        }
    }
}