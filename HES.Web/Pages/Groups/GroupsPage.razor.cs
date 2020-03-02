using System;
using System.Timers;
using HES.Core.Enums;
using HES.Core.Entities;
using HES.Web.Components;
using HES.Core.Interfaces;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using HES.Core.Models.Web.Breadcrumb;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace HES.Web.Pages.Groups
{
    public partial class GroupsPage : ComponentBase
    {
        [Inject] 
        public IGroupService GroupService { get; set; }
        
        [Inject] 
        public ILogger<GroupsPage> Logger { get; set; }

        public IList<Group> Groups { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _timer = new Timer(700);
            _timer.Elapsed += async (sender, args) => { await FilterRecordsAsync();};
            _timer.AutoReset = false;

            await InitializeComponentAsync();
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
            return await GroupService.GetAllGroupsAsync((CurrentPage - 1) * PageSize, PageSize, dir, SearchString, columnName);
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

        #region Pagination

        public int PagerSize { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }

        public void SetPagerSize(PagerDirection pagerDirection)
        {
            if (pagerDirection == PagerDirection.Next && EndPage < TotalPages)
            {
                StartPage = EndPage + 1;
                if (EndPage + PagerSize < TotalPages)
                {
                    EndPage = StartPage + PagerSize - 1;
                }
                else
                {
                    EndPage = TotalPages;
                }
                StateHasChanged();
            }
            else if (pagerDirection == PagerDirection.Previous && StartPage > 1)
            {
                EndPage = StartPage - 1;
                StartPage = StartPage - PagerSize;
            }
            else
            {
                StartPage = 1;
                EndPage = TotalPages;
            }
        }

        public async Task NavigateToPage(PagerDirection pagerDirection)
        {
            if (pagerDirection == PagerDirection.Next)
            {
                if (CurrentPage < TotalPages)
                {
                    if (CurrentPage == EndPage)
                    {
                        SetPagerSize(PagerDirection.Next);
                    }
                    CurrentPage += 1;
                }
            }
            else if (pagerDirection == PagerDirection.Previous)
            {
                if (CurrentPage > 1)
                {
                    if (CurrentPage == StartPage)
                    {
                        SetPagerSize(PagerDirection.Previous);
                    }
                    CurrentPage -= 1;
                }
            }

            await RefreshRecords(CurrentPage);
        }

        public async Task RefreshRecords(int currentPage)
        {
            CurrentPage = currentPage;
            await LoadGroupsAsync();
        }

        public async Task RefreshTable()
        {
            TotalRecords = await GroupService.GetCountAsync(SearchString);
            TotalPages = (int)Math.Ceiling(TotalRecords / (decimal)PageSize);
            CurrentPage = 1;
            PagerSize = TotalPages < 3 ? TotalPages : 3;
            StartPage = 1;
            EndPage = PagerSize;
            await LoadGroupsAsync();
        }

        public async Task ShowEntriesOnChange(ChangeEventArgs args)
        {
            PageSize = Convert.ToInt32(args.Value);
            await RefreshTable();
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

        public async Task FilterRecordsAsync()
        {
            await InvokeAsync(async () =>
            {
                await RefreshTable();
                StateHasChanged();
            });
        }

        #endregion

        public async Task InitializeComponentAsync()
        {
            PagerSize = 3;
            PageSize = 10;
            CurrentPage = 1;
            await LoadGroupsAsync();
            TotalRecords = await GroupService.GetCountAsync(SearchString);
            TotalPages = (int)Math.Ceiling(TotalRecords / (decimal)PageSize);
            SetPagerSize(PagerDirection.Next);
        }

        public async Task LoadGroupsAsync()
        {
            Groups = await GroupService.GetAllGroupsAsync((CurrentPage - 1) * PageSize, PageSize, ListSortDirection.Ascending, SearchString);
            CurrentGroupId = null;
        }

        public void RowSelected(string groupId)
        {
            CurrentGroupId = groupId != CurrentGroupId ? groupId : null;
        }


        #region PageUiMethods

        public async Task CreateBreadcrumbsAsync()
        {
            var items = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = "active", Content = "Groups" }
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
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadGroupsAsync));
                builder.CloseComponent();
            };

            await MainWrapper.ModalDialogComponent.ShowAsync("Create group", body);
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

            await MainWrapper.ModalDialogComponent.ShowAsync("Edit group", body);
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

            await MainWrapper.ModalDialogComponent.ShowAsync("Delete group", body);
        }

        public async Task OpenModalManageEmployees()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ManageEmployees));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadGroupsAsync));
                builder.AddAttribute(2, "GroupId", CurrentGroupId);
                builder.CloseComponent();
            };

            await MainWrapper.ModalDialogComponent.ShowAsync("Manage employees", body);
        }

        #endregion
    }
}
