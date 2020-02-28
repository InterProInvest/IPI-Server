using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.ActiveDirectory;
using HES.Core.Models.Web;
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
        public List<CheckboxWrapper<Group>> Groups { get; set; }
        public bool OnlyUserGroups { get; set; }
        public bool IsBusy { get; set; }
        public bool NotSelected { get; set; }
        public ActiveDirectoryLogin Login = new ActiveDirectoryLogin();

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

                Groups = new List<CheckboxWrapper<Group>>();
                foreach (var item in groups)
                {
                    Groups.Add(new CheckboxWrapper<Group>() { Model = item, Checked = false });
                }
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
                if (!Groups.Any(x => x.Checked == true))
                {
                    NotSelected = true;
                    return;
                }

                IsBusy = true;

                if (OnlyUserGroups)
                {
                    Groups.RemoveAll(x => x.Model.IsUserGroup == false);
                }

                var groups = new List<Group>();
                foreach (var item in Groups)
                {
                    if (item.Checked)
                    {
                        groups.Add(item.Model);
                    }
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

        private void OnRowSelected(string Id)
        {
            var checkbox = Groups.FirstOrDefault(x => x.Model.Id == Id);
            checkbox.Checked = !checkbox.Checked;
        }

        public void OnChangeCheckAll(ChangeEventArgs args)
        {
            Groups.ForEach(x => x.Checked = (bool)args.Value);
        }
    }
}
