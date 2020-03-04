using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Breadcrumb;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;

namespace HES.Web.Pages.Groups
{
    public partial class GroupsPage : ComponentBase
    {
        [Inject] public IGroupService GroupService { get; set; }
        [Inject] public ILogger<GroupsPage> Logger { get; set; }
        
        public int DisplayRows { get; set; }
        public int CurrentPage { get; set; }
        public int TotalRecords { get; set; }

        public IList<Group> Groups { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _timer = new Timer(700);
            _timer.Elapsed += async (sender, args) =>
            {
                await InvokeAsync(async () =>
                {
                    await RefreshTable();
                    StateHasChanged();
                });
            };
            _timer.AutoReset = false;

            await RefreshTable();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await CreateBreadcrumbsAsync();
        }

        #region SortTable

        private bool _isSortedAscending;
        private string _activeSortColumn;
        public string CurrentGroupId { get; set; }
        
        public async Task<IList<Group>> SortRecords(string columnName, ListSortDirection dir)
        {
            return await GroupService.GetAllGroupsAsync((CurrentPage - 1) * DisplayRows, DisplayRows, dir, SearchString, columnName);
        }

        public async Task SortTable(string columnName)
        {
            if (columnName != _activeSortColumn)
            {
                Groups = await SortRecords(columnName, ListSortDirection.Descending);
            }
            else
            {
                if (_isSortedAscending)
                {
                    Groups = await SortRecords(columnName, ListSortDirection.Ascending);
                }
                else
                {
                    Groups = await SortRecords(columnName, ListSortDirection.Descending);
                }
            }

            _activeSortColumn = columnName;
            _isSortedAscending = !_isSortedAscending;
        }

        public string SetSortIcon(string columnName)
        {
            if (_activeSortColumn != columnName)
            {
                return string.Empty;
            }

            if (_isSortedAscending)
            {
                return "sort-arrow-up";
            }
            else
            {
                return "sort-arrow-down";
            }
        }

        #endregion

        #region Search

        private string SearchString { get; set; }
        private Timer _timer;

        public void HandleKeyUp(KeyboardEventArgs e)
        {
            _timer.Stop();
            _timer.Start();
        }

        #endregion
                
        //Refresh table from pagination
        public async Task RefreshTable(int currentPage, int displayRows)
        {
            TotalRecords = await GroupService.GetCountAsync(SearchString);
            DisplayRows = displayRows;
            CurrentPage = currentPage;
            await LoadGroupsAsync();
            
        }

        //Internal refresh for CRUD
        public async Task RefreshTable()
        {
            await RefreshTable(1, 10);
        }

        public async Task LoadGroupsAsync()
        {
            Groups = await GroupService.GetAllGroupsAsync((CurrentPage - 1) * DisplayRows, DisplayRows, ListSortDirection.Ascending, SearchString);
            StateHasChanged();
        }

        public void OnRowSelected(string groupId)
        {
            CurrentGroupId = groupId;
        }

        private void GroupDetails()
        {
            NavigationManager.NavigateTo($"/Groups/Details?id={CurrentGroupId}", true);
        }

        public async Task CreateBreadcrumbsAsync()
        {
            var items = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Groups" }
            };
            await BreadcrumbsWrapper.BreadcrumbsComponent.ShowAsync(items);
        }

        public async Task OpenModalAddGroup()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, RefreshTable));
                builder.CloseComponent();
            };

            await MainWrapper.ModalDialogComponent.ShowAsync("Add group from AD", body);
        }

        public async Task OpenModalGreateGroup()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, RefreshTable));
                builder.CloseComponent();
            };

            await MainWrapper.ModalDialogComponent.ShowAsync("Create group", body);
        }

        public async Task OpenModalEditGroup()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, RefreshTable));
                builder.AddAttribute(2, "GroupId", CurrentGroupId);
                builder.CloseComponent();
            };

            await MainWrapper.ModalDialogComponent.ShowAsync("Edit group", body);
        }

        public async Task OpenModalDeleteGroup()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, RefreshTable));
                builder.AddAttribute(2, "GroupId", CurrentGroupId);
                builder.CloseComponent();
            };

            await MainWrapper.ModalDialogComponent.ShowAsync("Delete group", body);
        }
    }
}