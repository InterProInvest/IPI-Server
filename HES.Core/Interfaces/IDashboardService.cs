using HES.Core.Entities;
using HES.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDashboardService
    {
        string GetServerVersion();
        Task<int> GetDeviceTasksCount();
        Task<List<DeviceTask>> GetDeviceTasks();
        Task<List<DashboardNotify>> GetServerNotify();
        Task<int> GetEmployeesCount();
        Task<int> GetEmployeesOpenedSessionsCount();
        Task<List<DashboardNotify>> GetEmployeesNotify();
        Task<int> GetDevicesCount();
        Task<int> GetFreeDevicesCount();
        Task<List<DashboardNotify>> GetDevicesNotify();
        Task<int> GetWorkstationsCount();
        Task<int> GetWorkstationsOnlineCount();
        Task<List<DashboardNotify>> GetWorkstationsNotify();
    }
}
