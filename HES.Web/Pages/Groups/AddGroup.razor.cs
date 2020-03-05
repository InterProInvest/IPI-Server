using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.ActiveDirectory;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class AddGroup : ComponentBase
    {
        [Inject] public ILdapService LdapService { get; set; }
        [Inject] public IGroupService GroupService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<AddGroup> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public List<Group> Groups { get; set; }
        public List<Group> SelectedGroups { get; set; }

        private ActiveDirectoryLogin _login = new ActiveDirectoryLogin();
        private string _warningMessage { get; set; }
        private bool _isBusy { get; set; }

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
                Groups = LdapService.GetAdGroups(_login.Server, _login.UserName, _login.Password);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        private Task CollectionChanged(List<Group> groups)
        {
            SelectedGroups = groups;
            return Task.CompletedTask;
        }

        private async Task AddAsync()
        {
            try
            {
                if (SelectedGroups == null || SelectedGroups.Count == 0)
                {
                    _warningMessage = "Please select at least one employee.";
                    return;
                }

                if (_isBusy)
                {
                    return;
                }

                _isBusy = true;

                await GroupService.CreateGroupRangeAsync(SelectedGroups);
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Groups added.", ToastLevel.Success);
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
    }
}