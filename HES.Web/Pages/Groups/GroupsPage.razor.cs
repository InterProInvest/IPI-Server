using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Breadcrumb;
using HES.Core.Models.Web.Group;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
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
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }

        public IList<Group> Groups { get; set; }
        public string CurrentGroupId { get; set; }

        #region Search

        public int Delay { get; set; } = 500;
        public string Placeholder { get; set; } = "Search";
        public string SearchText { get; set; } = string.Empty;

        private Timer _timer;

        #endregion

        #region Filter

        public GroupFilter GroupFilter = new GroupFilter();

        #endregion

        #region Pagination

        public int DisplayRows { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
        public int TotalRecords { get; set; }

        #endregion

        #region Sorting

        public string SortColumn { get; set; } = nameof(Group.Name);
        public ListSortDirection SortDirection { get; set; } = ListSortDirection.Ascending;

        #endregion

        protected override async Task OnInitializedAsync()
        {
            await LoadGroupsAsync();
            CreateSearchTimer();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var items = new List<Breadcrumb>()
                {
                    new Breadcrumb () { Active = true, Content = "Groups" }
                };
                await JSRuntime.InvokeVoidAsync("createBreadcrumbs", items);
            }
        }

        #region Search

        private void CreateSearchTimer()
        {
            _timer = new Timer(Delay);
            _timer.Elapsed += async (sender, args) =>
            {
                await InvokeAsync(async () =>
                {
                    await LoadGroupsAsync();
                });
            };
            _timer.AutoReset = false;
        }

        private void OnKeyUp(KeyboardEventArgs e)
        {
            _timer.Stop();
            _timer.Start();
        }

        #endregion

        #region Filter

        private async Task FilterGroupsAsync()
        {
            await LoadGroupsAsync();
        }

        private async Task ClearFilterAsync()
        {
            GroupFilter = new GroupFilter();
            await LoadGroupsAsync();
        }

        #endregion

        #region SortTable

        private async Task SortTable(string columnName)
        {
            if (columnName != SortColumn)
            {
                SortColumn = columnName;
                await LoadGroupsAsync();
            }
            else
            {
                SortDirection = SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                await LoadGroupsAsync();
            }
        }

        private string SetSortIcon(string columnName)
        {
            if (SortColumn != columnName)
            {
                return string.Empty;
            }

            if (SortDirection == ListSortDirection.Ascending)
            {
                return "table-sort-arrow-up";
            }
            else
            {
                return "table-sort-arrow-down";
            }
        }

        #endregion

        #region Pagination

        //private async Task RefreshTable(int currentPage, int displayRows)
        //{
        //    DisplayRows = displayRows;
        //    CurrentPage = currentPage;
        //    await LoadGroupsAsync();
        //}

        private async Task CurrentPageChanged(int currentPage)
        {
            CurrentPage = currentPage;
            await LoadGroupsAsync();
        }
        private async Task DisplayRowsChanged(int displayRows)
        {
            DisplayRows = displayRows;
            CurrentPage = 1; // TODO calc current page if display rows changed
            await LoadGroupsAsync();
        }

        #endregion

        private async Task LoadGroupsAsync()
        {
            var currentTotalRows = TotalRecords;
            TotalRecords = await GroupService.GetCountAsync(SearchText, GroupFilter);

            if (currentTotalRows != TotalRecords)
                CurrentPage = 1;

            Groups = await GroupService.GetAllGroupsAsync((CurrentPage - 1) * DisplayRows, DisplayRows, SortColumn, SortDirection, SearchText, GroupFilter);
            CurrentGroupId = null;
            StateHasChanged();
        }

        private void OnRowSelected(string groupId)
        {
            CurrentGroupId = groupId;
        }

        private void GroupDetails()
        {
            NavigationManager.NavigateTo($"/Groups/Details?id={CurrentGroupId}", true);
        }

        private async Task OpenModalAddGroup()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadGroupsAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add group", body);
        }

        private async Task OpenModalGreateGroup()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadGroupsAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Create group", body);
        }

        private async Task OpenModalEditGroup()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadGroupsAsync));
                builder.AddAttribute(2, "GroupId", CurrentGroupId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Edit group", body);
        }

        private async Task OpenModalDeleteGroup()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadGroupsAsync));
                builder.AddAttribute(2, "GroupId", CurrentGroupId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete group", body);
        }
    }
}