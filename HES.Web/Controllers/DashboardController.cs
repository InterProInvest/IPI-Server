using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models;
using HES.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.AdminRole)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [HttpGet]
        public string GetServerVersion()
        {
            return _dashboardService.GetServerVersion();
        }

        [HttpGet]
        public async Task<int> GetDeviceTasksCount()
        {
            return await _dashboardService.GetDeviceTasksCount();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceTask>>> GetDeviceTasks()
        {
            return await _dashboardService.GetDeviceTasks();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DashboardNotify>>> GetServerNotify()
        {
            return await _dashboardService.GetServerNotifyAsync();
        }

        [HttpGet]
        public async Task<int> GetEmployeesCount()
        {
            return await _dashboardService.GetEmployeesCountAsync();
        }

        [HttpGet]
        public async Task<int> GetEmployeesOpenedSessionsCount()
        {
            return await _dashboardService.GetEmployeesOpenedSessionsCountAsync();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DashboardNotify>>> GetEmployeesNotify()
        {
            return await _dashboardService.GetEmployeesNotifyAsync();
        }

        [HttpGet]
        public async Task<int> GetDevicesCount()
        {
            return await _dashboardService.GetDevicesCountAsync();
        }

        [HttpGet]
        public async Task<int> GetFreeDevicesCount()
        {
            return await _dashboardService.GetFreeDevicesCountAsync();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DashboardNotify>>> GetDevicesNotify()
        {
            return await _dashboardService.GetDevicesNotifyAsync();
        }

        [HttpGet]
        public async Task<int> GetWorkstationsCount()
        {
            return await _dashboardService.GetWorkstationsCountAsync();
        }

        [HttpGet]
        public async Task<int> GetWorkstationsOnlineCount()
        {
            return await _dashboardService.GetWorkstationsOnlineCountAsync();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DashboardNotify>>> GetWorkstationsNotify()
        {
            return await _dashboardService.GetWorkstationsNotifyAsync();
        }
    }
}