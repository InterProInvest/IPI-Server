﻿using HES.Core.Entities;
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

namespace HES.Web.Pages.Groups
{
    public partial class AddGroup : ComponentBase
    {
        [Inject] public ILdapService LdapService { get; set; }
        [Inject] public IGroupService GroupService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<AddGroup> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public List<ActiveDirectoryGroup> Groups { get; set; }
        public LdapSettings LdapSettings { get; set; }
        public ActiveDirectoryInitialization ActiveDirectoryInitialization { get; set; }
        public string WarningMessage { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public bool IsSortedAscending { get; set; } = true;
        public string CurrentSortColumn { get; set; } = nameof(Group.Name);
        public bool CreateEmployees { get; set; }
        public bool Initialized { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                LdapSettings = await AppSettingsService.GetLdapSettingsAsync();

                if (LdapSettings == null)
                {
                    ActiveDirectoryInitialization = ActiveDirectoryInitialization.HostNotSet;
                }
                else if (LdapSettings?.Host != null && LdapSettings?.UserName == null && LdapSettings?.Password == null)
                {
                    ActiveDirectoryInitialization = ActiveDirectoryInitialization.CredentialsNotSet;
                }
                else
                {
                    await GetGroups(LdapSettings);
                }

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }


        private async Task GetGroups(LdapSettings settings)
        {
            try
            {
                Groups = await LdapService.GetGroupsAsync(settings);
                ActiveDirectoryInitialization = ActiveDirectoryInitialization.Loaded;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task AddAsync()
        {
            try
            {
                if (!Groups.Any(x => x.Checked))
                {
                    WarningMessage = "Please select at least one group.";
                    return;
                }

                await LdapService.AddGroupsAsync(Groups.Where(x => x.Checked).ToList(), CreateEmployees);
                await ToastService.ShowToastAsync("Groups added.", ToastType.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Groups);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private string GetSortIcon(string columnName)
        {
            if (CurrentSortColumn != columnName)
            {
                return string.Empty;
            }
            if (IsSortedAscending)
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
            if (columnName != CurrentSortColumn)
            {
                Groups = Groups.OrderBy(x => x.Group.GetType().GetProperty(columnName).GetValue(x.Group, null)).ToList();
                CurrentSortColumn = columnName;
                IsSortedAscending = true;
            }
            else
            {
                if (IsSortedAscending)
                {
                    Groups = Groups.OrderByDescending(x => x.Group.GetType().GetProperty(columnName).GetValue(x.Group, null)).ToList();
                }
                else
                {
                    Groups = Groups.OrderBy(x => x.Group.GetType().GetProperty(columnName).GetValue(x.Group, null)).ToList();
                }

                IsSortedAscending = !IsSortedAscending;
            }
        }
    }
}