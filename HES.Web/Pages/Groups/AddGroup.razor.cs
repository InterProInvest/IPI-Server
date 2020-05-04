using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.ActiveDirectory;
using HES.Core.Models.Web.AppSettings;
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
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public List<ActiveDirectoryGroup> Groups { get; set; }
        public DomainSettings Domain { get; set; }
        public string WarningMessage { get; set; }

        private ActiveDirectoryCredential _credentials = new ActiveDirectoryCredential();
        private bool _createEmployees;
        private bool _isBusy;
        private string _searchText = string.Empty;
        private bool _isSortedAscending = true;
        private string _currentSortColumn = nameof(Group.Name);

        protected override async Task OnInitializedAsync()
        {
            Domain = await AppSettingsService.GetDomainSettingsAsync();

            if (Domain != null)
                _credentials.Host = Domain.Host;
        }

        private async Task Connect()
        {
            if (_isBusy)
            {
                return;
            }

            _isBusy = true;

            try
            {
                Groups = await LdapService.GetGroupsAsync(_credentials);
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
                if (!Groups.Any(x => x.Checked))
                {
                    WarningMessage = "Please select at least one group.";
                    return;
                }

                if (_isBusy)
                {
                    return;
                }

                _isBusy = true;

                await LdapService.AddGroupsAsync(Groups.Where(x => x.Checked).ToList(), _createEmployees);
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
                Groups = Groups.OrderBy(x => x.Group.GetType().GetProperty(columnName).GetValue(x.Group, null)).ToList();
                _currentSortColumn = columnName;
                _isSortedAscending = true;
            }
            else
            {
                if (_isSortedAscending)
                {
                    Groups = Groups.OrderByDescending(x => x.Group.GetType().GetProperty(columnName).GetValue(x.Group, null)).ToList();
                }
                else
                {
                    Groups = Groups.OrderBy(x => x.Group.GetType().GetProperty(columnName).GetValue(x.Group, null)).ToList();
                }

                _isSortedAscending = !_isSortedAscending;
            }
        }
    }
}