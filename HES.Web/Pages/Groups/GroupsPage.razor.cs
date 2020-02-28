using System;
using System.Linq;
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

namespace HES.Web.Pages.Groups
{
    public partial class GroupsPage : ComponentBase
    {
        #region Di

        [Inject] 
        public IGroupService GroupService { get; set; }
        
        [Inject] 
        public ILogger<GroupsPage> Logger { get; set; }

        #endregion

        #region Properties
        
        public IList<Group> Groups { get; set; }
        public string CurrentGroupId { get; set; }

        #endregion

        #region LifeCycleHooks

        protected override async Task OnInitializedAsync()
        {
            await InitializeComponentAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await CreateBreadcrumbsAsync();
        }

        #endregion

        #region SortTable

        private bool _isSortedAscending;
        private string _activeSortColumn;

        public async Task<IList<Group>> SortRecords(string columnName, ListSortDirection dir)
        {
            return await GroupService.GetAllGroupsAsync((CurrentPage - 1) * PageSize, PageSize, dir, _searchString, columnName);
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

        public async Task ShowEntriesOnChange(ChangeEventArgs args)
        {
            PageSize = Convert.ToInt32(args.Value);
            TotalPages = (int)Math.Ceiling(TotalRecords / (decimal)PageSize);
            CurrentPage = 1;
            PagerSize = TotalPages < 3 ? TotalPages : 3;
            StartPage = 1;
            EndPage = PagerSize;

            await LoadGroupsAsync();
        }

        #endregion

        private string _searchString;

        public async Task FilterRecordsAsync(ChangeEventArgs args)
        {
            _searchString = (string)args.Value;
            EndPage = 0;
            await InitializeComponentAsync();
        }

        public async Task InitializeComponentAsync()
        {
            PagerSize = 3;
            PageSize = 10;
            CurrentPage = 1;
            await LoadGroupsAsync();
            TotalRecords = await GroupService.GetCountAsync(_searchString);
            TotalPages = (int)Math.Ceiling(TotalRecords / (decimal)PageSize);
            SetPagerSize(PagerDirection.Next);
        }

        public async Task LoadGroupsAsync()
        {
            Groups = await GroupService.GetAllGroupsAsync((CurrentPage - 1) * PageSize, PageSize, ListSortDirection.Descending, _searchString);
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

        #endregion
    }
}
