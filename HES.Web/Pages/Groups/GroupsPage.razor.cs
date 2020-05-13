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

        public List<Group> Groups { get; set; }
        public Group SelectedGroup { get; set; }
        public GroupFilter Filter { get; set; } = new GroupFilter();
        public string SearchText { get; set; } = string.Empty;
        public string SortedColumn { get; set; } = nameof(Group.Name);
        public ListSortDirection SortDirection { get; set; } = ListSortDirection.Ascending;
        public int DisplayRows { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
        public int TotalRecords { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadTableDataAsync();            
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

        #region Main Table

        private async Task LoadTableDataAsync()
        {
            var currentTotalRows = TotalRecords;
            TotalRecords = await GroupService.GetGroupsCountAsync(SearchText, Filter);

            if (currentTotalRows != TotalRecords)
                CurrentPage = 1;

            Groups = await GroupService.GetGroupsAsync((CurrentPage - 1) * DisplayRows, DisplayRows, SortedColumn, SortDirection, SearchText, Filter);
            SelectedGroup = null;

            StateHasChanged();
        }

        private async Task SelectedItemChangedAsync(Group group)
        {
            await InvokeAsync(() =>
            {
                SelectedGroup = group;
                StateHasChanged();
            });
        }

        private async Task SortedColumnChangedAsync(string columnName)
        {
            SortedColumn = columnName;
            await LoadTableDataAsync();
        }

        private async Task SortDirectionChangedAsync(ListSortDirection sortDirection)
        {
            SortDirection = sortDirection;
            await LoadTableDataAsync();
        }

        private async Task CurrentPageChangedAsync(int currentPage)
        {
            CurrentPage = currentPage;
            await LoadTableDataAsync();
        }

        private async Task DisplayRowsChangedAsync(int displayRows)
        {
            DisplayRows = displayRows;
            CurrentPage = 1;
            await LoadTableDataAsync();
        }

        private async Task SearchTextChangedAsync(string searchText)
        {
            SearchText = searchText;
            await LoadTableDataAsync();
        }

        private async Task FilterChangedAsync(GroupFilter filter)
        {
            Filter = filter;
            await LoadTableDataAsync();
        }

        #endregion

        private Task NavigateToGroupDetails()
        {
            NavigationManager.NavigateTo($"/Groups/Details?id={SelectedGroup.Id}", true);
            return Task.CompletedTask;
        }

        private async Task OpenModalAddGroupAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add group", body);
        }

        private async Task OpenModalCreateGroupAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Create group", body);
        }

        private async Task OpenModalEditGroupAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.AddAttribute(2, "GroupId", SelectedGroup.Id);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Edit group", body);
        }

        private async Task OpenModalDeleteGroupAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteGroup));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.AddAttribute(2, "GroupId", SelectedGroup.Id);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete group", body);
        }
    }
}