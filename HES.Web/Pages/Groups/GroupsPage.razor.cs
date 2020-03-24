using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Breadcrumb;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;

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
                await CreateBreadcrumbsAsync();
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

        public void OnKeyUp(KeyboardEventArgs e)
        {
            _timer.Stop();
            _timer.Start();
        }

        #endregion

        #region SortTable

        public async Task SortTable(string columnName)
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

        public string SetSortIcon(string columnName)
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

        public async Task RefreshTable(int currentPage, int displayRows)
        {
            DisplayRows = displayRows;
            CurrentPage = currentPage;
            await LoadGroupsAsync();
        }

        #endregion
                
        public async Task LoadGroupsAsync()
        {
            TotalRecords = await GroupService.GetCountAsync(SearchText);

            if (!string.IsNullOrWhiteSpace(SearchText) && TotalRecords > 0)
            {
                CurrentPage = 1;
            }

            Groups = await GroupService.GetAllGroupsAsync((CurrentPage - 1) * DisplayRows, DisplayRows, SortColumn, SortDirection, SearchText);
            CurrentGroupId = null;
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
            await JSRuntime.InvokeVoidAsync("createBreadcrumbs", items);
        }

        public async Task OpenModalAddGroup()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadGroupsAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add group", body);
        }

        public async Task OpenModalGreateGroup()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadGroupsAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Create group", body);
        }

        public async Task OpenModalEditGroup()
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

        public async Task OpenModalDeleteGroup()
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