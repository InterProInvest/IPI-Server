using System;
using System.Linq;
using HES.Core.Enums;
using HES.Core.Entities;
using HES.Web.Components;
using HES.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components;
using HES.Core.Models.ActiveDirectory;

namespace HES.Web.Pages.Groups
{
    public partial class AddGroup : ComponentBase
    {
        [Inject] public ILdapService LdapService { get; set; }
        [Inject] public IGroupService GroupService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<AddGroup> Logger { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public bool OnlyUserGroups { get; set; }
        public bool IsBusy { get; set; }
        public bool NotSelected { get; set; }
        public ActiveDirectoryLogin Login = new ActiveDirectoryLogin();

        public bool IsSelectAll { get; set; }

        public Dictionary<Group, bool> Groups = new Dictionary<Group, bool>();

        protected override async Task OnInitializedAsync()
        {
            var domain = await AppSettingsService.GetDomainSettingsAsync();
            if (domain != null)
            {
                Login.Server = domain.IpAddress;
            }
        }

        private async Task Connect()
        {
            try
            {
                var groups = LdapService.GetAdGroups(Login.Server, Login.UserName, Login.Password);
                Groups = groups.ToDictionary(k => k, v => false);
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
                if (!Groups.Any(x => x.Value == true))
                {
                    NotSelected = true;
                    return;
                }

                IsBusy = true;

                List<Group> groups;

                if (OnlyUserGroups)
                {
                    groups = Groups.Keys.Where(x => x.IsUserGroup).ToList();
                }
                else
                {
                    groups = Groups.Where(x => x.Value).Select(x => x.Key).ToList();
                }

                await GroupService.CreateGroupRangeAsync(groups);
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
        }

        private void OnRowSelected(Group key)
        {
            Groups[key] = !Groups[key];
        }

        public void OnChangeCheckAll(ChangeEventArgs args)
        {
            IsSelectAll = !IsSelectAll;
            foreach (var key in Groups.Keys.ToList())
                Groups[key] = IsSelectAll;
        }
    }
}
