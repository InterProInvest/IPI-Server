using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.ActiveDirectory;
using HES.Core.Models.Web.AppSettings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class AddEmployee : ComponentBase
    {
        [Inject] public ILdapService LdapService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<AddEmployee> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IHubContext<EmployeesHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public List<ActiveDirectoryUser> Users { get; set; }
        public LdapSettings LdapSettings { get; set; }
        public string WarningMessage { get; set; }

        private bool _isBusy;
        private string _searchText = string.Empty;
        private bool _isSortedAscending = true;
        private string _currentSortColumn = nameof(Employee.FullName);
        private bool _createGroups;
        private bool _initialized;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                LdapSettings = await AppSettingsService.GetLdapSettingsAsync();

                if (LdapSettings != null)
                {
                    Users = await LdapService.GetUsersAsync(LdapSettings);
                }

                _initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        private async Task AddAsync()
        {
            try
            {
                if (_isBusy)
                    return;

                _isBusy = true;

                if (!Users.Any(x => x.Checked))
                {
                    WarningMessage = "Please select at least one user.";
                    return;
                }

                await LdapService.AddUsersAsync(Users.Where(x => x.Checked).ToList(), _createGroups);
                ToastService.ShowToast("Employee imported.", ToastLevel.Success);
                await HubContext.Clients.All.SendAsync("PageUpdated", ConnectionId);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
            }
            finally
            {
                _isBusy = false;
            }
        }

        private string GetSortIcon(string columnName)
        {
            if (_currentSortColumn != columnName)
            {
                return string.Empty;
            }
            if (_isSortedAscending)
            {
                return "table-sort-arrow-up";
            }
            else
            {
                return "table-sort-arrow-down";
            }
        }

        private void SortTable(string columnName)
        {
            if (columnName != _currentSortColumn)
            {
                Users = Users.OrderBy(x => x.Employee.GetType().GetProperty(columnName).GetValue(x.Employee, null)).ToList();
                _currentSortColumn = columnName;
                _isSortedAscending = true;
            }
            else
            {
                if (_isSortedAscending)
                {
                    Users = Users.OrderByDescending(x => x.Employee.GetType().GetProperty(columnName).GetValue(x.Employee, null)).ToList();
                }
                else
                {
                    Users = Users.OrderBy(x => x.Employee.GetType().GetProperty(columnName).GetValue(x.Employee, null)).ToList();
                }

                _isSortedAscending = !_isSortedAscending;
            }
        }
    }
}