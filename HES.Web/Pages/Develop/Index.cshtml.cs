﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HES.Web.Pages.Develop
{
    public class IndexModel : PageModel
    {
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly IDeviceService _deviceService;
        private readonly IEmployeeService _employeeService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly IDeviceAccountService _deviceAccountService;

        public IList<DeviceTask> DeviceTasks { get; set; }
        public AccountModel AccountModel { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(IDeviceTaskService deviceTaskService,
                            IDeviceService deviceService,
                            IEmployeeService employeeService,
                            IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                            IDeviceAccountService deviceAccountService)
        {
            _deviceTaskService = deviceTaskService;
            _deviceService = deviceService;
            _employeeService = employeeService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _deviceAccountService = deviceAccountService;
        }

        public async Task OnGet()
        {
            DeviceTasks = await _deviceTaskService.Query().OrderByDescending(o => o.CreatedAt).ToListAsync();
            ViewData["Devices"] = new SelectList(await _deviceService.Query().OrderBy(c => c.Id).ToListAsync(), "Id", "Id");
            ViewData["Employee"] = new SelectList(await _employeeService.Query().OrderBy(c => c.FirstName).ToListAsync(), "Id", "FullName");
        }

        public async Task<IActionResult> OnPostCreateAccountAsync(AccountModel accountModel)
        {
            if (accountModel.DeviceId == null)
            {
                ErrorMessage = "DeviceId is null";
                return RedirectToPage("./index");
            }
            if (accountModel.EmployeeId == null)
            {
                ErrorMessage = "EmployeeId is null";
                return RedirectToPage("./index");
            }

            for (int i = 0; i < accountModel.AccountsCount; i++)
            {
                var deviceAccount = new DeviceAccount()
                {
                    Name = "Test_" + Guid.NewGuid().ToString(),
                    Urls = Guid.NewGuid().ToString(),
                    Apps = Guid.NewGuid().ToString(),
                    Login = Guid.NewGuid().ToString(),
                    EmployeeId = accountModel.EmployeeId
                };

                var input = new InputModel()
                {
                    Password = Guid.NewGuid().ToString(),
                    OtpSecret = Guid.NewGuid().ToString()
                };

                var devices = new string[] { accountModel.DeviceId };

                await _employeeService.CreatePersonalAccountAsync(deviceAccount, input, devices);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devices);
            }

            return RedirectToPage("./index");
        }

        public async Task<IActionResult> OnPostDeleteAccountAsync(AccountModel accountModel)
        {
            if (accountModel.DeviceId == null)
            {
                ErrorMessage = "DeviceId is null";
                return RedirectToPage("./index");
            }

            var accounts = await _deviceAccountService
                .Query()
                .Where(d => d.DeviceId == accountModel.DeviceId && d.Name.Contains("Test_"))
                .ToListAsync();

            foreach (var item in accounts)
            {
                var deviceId = await _employeeService.DeleteAccount(item.Id);
            }
            _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(accountModel.DeviceId);

            return RedirectToPage("./index");
        }
    }

    public class AccountModel
    {
        public string DeviceId { get; set; }
        public string EmployeeId { get; set; }
        public int AccountsCount { get; set; }
    }
}