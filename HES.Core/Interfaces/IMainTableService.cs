﻿using HES.Core.Enums;
using HES.Core.Models.Web;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IMainTableService<TItem, TFilter> where TFilter : class, new()
    {
        DataLoadingOptions<TFilter> DataLoadingOptions { get; set; }
        public int CurrentPage { get; set; }
        public int TotalRecords { get; set; }
        public TItem SelectedEntity { get; set; }
        public List<TItem> Entities { get; set; }

        Task ShowModalAsync(string modalTitle, RenderFragment modalBody, ModalDialogSize modalSize = ModalDialogSize.Default);
        Task InvokeJsAsync(string functionName, params object[] args);

        void Initialize(Func<DataLoadingOptions<TFilter>, Task<List<TItem>>> getEntities, Func<DataLoadingOptions<TFilter>, Task<int>> getEntitiesCount, Action stateHasChanged, string sortedColumn);
        Task LoadTableDataAsync();
        Task SelectedItemChangedAsync(TItem item);
        Task SortedColumnChangedAsync(string columnName);
        Task SortDirectionChangedAsync(ListSortDirection sortDirection);
        Task CurrentPageChangedAsync(int currentPage);
        Task DisplayRowsChangedAsync(int displayRows);
        Task SearchTextChangedAsync(string searchText);
        Task FilterChangedAsync(TFilter filter);
    }
}