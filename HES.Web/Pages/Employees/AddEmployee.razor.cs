using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.ActiveDirectory;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class AddEmployee : ComponentBase
    {
        [Inject] public ILdapService LdapService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<AddEmployee> Logger { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }

        public List<ActiveDirectoryUser> ActiveDirectoryUsers { get; set; }

        private ActiveDirectoryLogin _login = new ActiveDirectoryLogin();
        private bool _isBusy { get; set; }
        private bool _notSelected { get; set; }

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
                ActiveDirectoryUsers = LdapService.GetAdUsers(_login.Server, _login.UserName, _login.Password);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await MainWrapper.ModalDialogComponent.CloseAsync();
            }
        }

        private async Task AddAsync()
        {
            try
            {
                _isBusy = true;
                await LdapService.AddAdUsersAsync(ActiveDirectoryUsers);
                NavigationManager.NavigateTo("/Employees", true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await MainWrapper.ModalDialogComponent.CloseAsync();
            }
            finally
            {
                _isBusy = false;
            }
        }
    }
}
