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

        public Dictionary<ActiveDirectoryUser, bool> ActiveDirectoryUsers { get; set; }

        private ActiveDirectoryLogin _login = new ActiveDirectoryLogin();
        private bool _createGroups;
        private bool _isSelectedAll;
        private bool _isBusy;
        private string _warningMessage;

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
            try
            {
                var users = LdapService.GetAdUsers(_login.Server, _login.UserName, _login.Password);
                ActiveDirectoryUsers = users.ToDictionary(k => k, v => false);
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
                if (!ActiveDirectoryUsers.Any(x => x.Value == true))
                {
                    _warningMessage = "Please select at least one employee.";
                    return;
                }

                if (_isBusy)
                {
                    return;
                }

                _isBusy = true;
                var users = ActiveDirectoryUsers.Where(x => x.Value).Select(x => x.Key).ToList();
                await LdapService.AddAdUsersAsync(users, _createGroups);
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

        private void OnRowSelected(ActiveDirectoryUser key)
        {
            ActiveDirectoryUsers[key] = !ActiveDirectoryUsers[key];
        }

        public void OnChangeCheckAll(ChangeEventArgs args)
        {
            _isSelectedAll = !_isSelectedAll;
            foreach (var key in ActiveDirectoryUsers.Keys.ToList())
                ActiveDirectoryUsers[key] = _isSelectedAll;
        }
    }
}
