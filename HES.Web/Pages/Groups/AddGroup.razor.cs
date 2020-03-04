using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.ActiveDirectory;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class AddGroup : ComponentBase
    {
        [Inject] public ILdapService LdapService { get; set; }
        [Inject] public IGroupService GroupService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<AddGroup> Logger { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public Dictionary<Group, bool> Groups = new Dictionary<Group, bool>();
        public List<Group> SelectedGroups { get; set; }

        private ActiveDirectoryLogin _login = new ActiveDirectoryLogin();
        private bool _onlyUserGroups { get; set; }
        private bool _notSelected { get; set; }
        private bool _isSelectedAll { get; set; }
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
                var groups = LdapService.GetAdGroups(_login.Server, _login.UserName, _login.Password);
                Groups = groups.ToDictionary(k => k, v => false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await MainWrapper.ModalDialogComponent.CloseAsync();
            }
        }

        private async Task CollectionChanged(List<Group> groups)
        {
            SelectedGroups = groups;
        }

        private async Task AddAsync()
        {
            try
            {
                //if (!Groups.Any(x => x.Value == true))
                //{
                //    _notSelected = true;
                //    return;
                //}
                if (SelectedGroups == null || SelectedGroups.Count == 0)
                {
                    _notSelected = true;
                    return;
                }

                if (_isBusy)
                {
                    return;
                }

                _isBusy = true;
                List<Group> groups;

                if (_onlyUserGroups)
                {
                    groups = Groups.Keys.Where(x => x.IsUserGroup).ToList();
                }
                else
                {
                    groups = Groups.Where(x => x.Value).Select(x => x.Key).ToList();
                }

                await GroupService.CreateGroupRangeAsync(SelectedGroups);
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Groups added.", ToastLevel.Success);
                await MainWrapper.ModalDialogComponent.CloseAsync();
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

        private void OnRowSelected(Group key)
        {
            Groups[key] = !Groups[key];
        }

        public void OnChangeCheckAll(ChangeEventArgs args)
        {
            _isSelectedAll = !_isSelectedAll;
            foreach (var key in Groups.Keys.ToList())
                Groups[key] = _isSelectedAll;
        }
    }
}
