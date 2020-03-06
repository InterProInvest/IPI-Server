using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Breadcrumb;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class GroupsPage : ComponentBase
    {
        [Inject] public IGroupService GroupService { get; set; }
        [Inject] public ILogger<GroupsPage> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }

        public int DisplayRows { get; set; }
        public int CurrentPage { get; set; }
        public int TotalRecords { get; set; }
        public string CurrentGroupId { get; set; }

        public IList<Group> Groups { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await RefreshTable();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await CreateBreadcrumbsAsync();
        }

        #region SortTable

        private bool _isSortedAscending;
        private string _activeSortColumn = nameof(Group.Name);

        public async Task SortTable(string columnName)
        {
            if (columnName != _activeSortColumn)
            {
                await LoadGroupsAsync(string.Empty, columnName, ListSortDirection.Descending);
            }
            else
            {
                if (_isSortedAscending)
                {
                    await LoadGroupsAsync(string.Empty, columnName, ListSortDirection.Ascending);
                }
                else
                {
                    await LoadGroupsAsync(string.Empty, columnName, ListSortDirection.Descending);
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
                return "table-sort-arrow-up";
            }
            else
            {
                return "table-sort-arrow-down";
            }
        }

        #endregion


        //Refresh table from pagination
        public async Task RefreshTable(int currentPage, int displayRows)
        {
            DisplayRows = displayRows;
            CurrentPage = currentPage;
            await LoadGroupsAsync();
        }

        //Internal refresh for CRUD
        public async Task RefreshTable()
        {
            await RefreshTable(1, 10);
        }

        //Internal refresh for Search
        public async Task RefreshTable(string searchString)
        {
            await LoadGroupsAsync(searchString);
        }

        public async Task LoadGroupsAsync(string searchString = null,
                                          string columnName = nameof(Group.Name),
                                          ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            TotalRecords = await GroupService.GetCountAsync(searchString);
            Groups = await GroupService.GetAllGroupsAsync((CurrentPage - 1) * DisplayRows, DisplayRows, sortDirection, searchString, columnName);
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

            await ModalDialogService.ShowAsync("Add group", body);
        }

        public async Task OpenModalGreateGroup()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, RefreshTable));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Create group", body);
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

            await ModalDialogService.ShowAsync("Edit group", body);
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

            await ModalDialogService.ShowAsync("Delete group", body);
        }
    }
}