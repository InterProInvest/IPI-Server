using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public class MainPageModule<T, TFilter> : ComponentBase where TFilter : class, new() where T : class
    {
        public TFilter Filter { get; set; }
        public string SearchText { get; set; }
        public ListSortDirection SortDirection { get; set; }
        public string SortedColumn { get; set; }
        public int DisplayRows { get; set; }
        public int CurrentPage { get; set; }
        public int TotalRecords { get; set; }

        public T SelectedEntity { get; set; }
        public List<T> Entities { get; set; }

        private Action _refreshUi;
        private readonly IDataLoader<T, TFilter> _dataLoader;

        private bool _isInitialized;

        public MainPageModule(IDataLoader<T, TFilter> dataLoader)
        {
            _dataLoader = dataLoader;
        }

        public void InitializeModule(Action refreshUiMethod, string sortedColumn = "Id")
        {
            if (_isInitialized == true)
                return;

            _refreshUi = refreshUiMethod;
            SortedColumn = sortedColumn;
            DisplayRows = 10;
            CurrentPage = 1;
            SortDirection = ListSortDirection.Ascending;
            SearchText = string.Empty;
            Filter = new TFilter();

            _isInitialized = true;
        }

        public async Task LoadTableDataAsync()
        {
            var currentTotalRows = TotalRecords;
            TotalRecords = await _dataLoader.GetEntitiesCountAsync(SearchText, Filter);

            if (currentTotalRows != TotalRecords)
                CurrentPage = 1;

            Entities = await _dataLoader.GetEntitiesAsync((CurrentPage - 1) * DisplayRows, DisplayRows, SortedColumn, SortDirection, SearchText, Filter);
            SelectedEntity = null;

            _refreshUi?.Invoke();
        }

        public async Task SelectedItemChangedAsync(T item)
        {
            SelectedEntity = item;

            _refreshUi?.Invoke();
        }

        public async Task SortedColumnChangedAsync(string columnName)
        {
            SortedColumn = columnName;
            await LoadTableDataAsync();
        }

        public async Task SortDirectionChangedAsync(ListSortDirection sortDirection)
        {
            SortDirection = sortDirection;
            await LoadTableDataAsync();
        }

        public async Task CurrentPageChangedAsync(int currentPage)
        {
            CurrentPage = currentPage;
            await LoadTableDataAsync();
        }

        public async Task DisplayRowsChangedAsync(int displayRows)
        {
            DisplayRows = displayRows;
            CurrentPage = 1;
            await LoadTableDataAsync();
        }

        public async Task SearchTextChangedAsync(string searchText)
        {
            SearchText = searchText;
            await LoadTableDataAsync();
        }

        public async Task FilterChangedAsync(TFilter filter)
        {
            Filter = filter;
            await LoadTableDataAsync();
        }
    }
}
