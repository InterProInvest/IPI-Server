using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.ActiveDirectory;
using Microsoft.AspNetCore.Components;
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

        public List<ActiveDirectoryUser> Users { get; set; }

        private ActiveDirectoryLogin _login = new ActiveDirectoryLogin();
        private bool _createGroups;
        private string _warningMessage;
        private bool _isBusy;
        private string _searchText = string.Empty;
        private bool _isSortedAscending = true;
        private string _currentSortColumn = nameof(Employee.FullName);

        protected override async Task OnInitializedAsync()
        {
            var domain = await AppSettingsService.GetDomainSettingsAsync();
            if (domain != null)
            {
                _login.Server = domain.IpAddress;
            }
        }

        private async Task Connect()
        {
            if (_isBusy)
            {
                return;
            }

            _isBusy = true;
            await Task.Delay(1); // To display a spinner, without await is not displayed
            
            try
            {
                Users = LdapService.GetAdUsers(_login.Server, _login.UserName, _login.Password);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async Task AddAsync()
        {
            try
            {
                if (!Users.Any(x => x.Checked))
                {
                    _warningMessage = "Please select at least one user.";
                    return;
                }

                if (_isBusy)
                {
                    return;
                }

                _isBusy = true;

                await LdapService.AddAdUsersAsync(Users.Where(x => x.Checked).ToList(), _createGroups);
                NavigationManager.NavigateTo("/Employees", true);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
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