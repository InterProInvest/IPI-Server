using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class MainTableService<TItem, TFilter> : ComponentBase, IDisposable, IMainTableService<TItem, TFilter> where TItem : class where TFilter : class, new()
    {
        private readonly IJSRuntime _jSRuntime;
        private readonly IModalDialogService _modalDialogService;

        public TFilter Filter { get; set; } = new TFilter();
        public string SearchText { get; set; } = string.Empty;
        public ListSortDirection SortDirection { get; set; } = ListSortDirection.Ascending;
        public int DisplayRows { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
        public string SortedColumn { get; set; }
        public int TotalRecords { get; set; }
        public TItem SelectedEntity { get; set; }
        public List<TItem> Entities { get; set; }

        private Func<string, TFilter, Task<int>> _getEntitiesCount;
        private Func<int, int, string, ListSortDirection, string, TFilter, Task<List<TItem>>> _getEntities;
        private Action _stateHasChanged;

        public MainTableService(IJSRuntime jSRuntime, IModalDialogService modalDialogService)
        {
            _jSRuntime = jSRuntime;
            _modalDialogService = modalDialogService;
        }

        public void Initialize(Func<int, int, string, ListSortDirection, string, TFilter, Task<List<TItem>>> getEntities, Func<string, TFilter, Task<int>> getEntitiesCount, Action stateHasChanged, string sortedColumn)
        {
            _stateHasChanged = stateHasChanged;
            _getEntities = getEntities;
            _getEntitiesCount = getEntitiesCount;
            SortedColumn = sortedColumn;

            _modalDialogService.OnClose += LoadTableDataAsync;
        }

        public async Task LoadTableDataAsync()
        {
            var currentTotalRows = TotalRecords;
            TotalRecords = await _getEntitiesCount.Invoke(SearchText, Filter);

            if (currentTotalRows != TotalRecords)
                CurrentPage = 1;

            Entities = await _getEntities.Invoke((CurrentPage - 1) * DisplayRows, DisplayRows, SortedColumn, SortDirection, SearchText, Filter);
            SelectedEntity = null;

            _stateHasChanged?.Invoke();
        }


        public async Task ShowModalAsync(string modalTitle, RenderFragment modalBody, ModalDialogSize modalSize = ModalDialogSize.Default)
        {
            await _modalDialogService.ShowAsync(modalTitle, modalBody, modalSize);
        }

        public async Task InvokeJsAsync(string functionName, params object[] args)
        {
            await _jSRuntime.InvokeVoidAsync(functionName, args);
        }

        public async Task<T> InvokeJsAsync<T>(string functionName, params object[] args)
        {
            return await _jSRuntime.InvokeAsync<T>(functionName, args);
        }


        #region Table Actions

        public Task SelectedItemChangedAsync(TItem item)
        {
            SelectedEntity = item;
            _stateHasChanged?.Invoke();
            return Task.CompletedTask;
        }
        public async Task FilterChangedAsync(TFilter filter)
        {
            Filter = filter;
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

        #endregion

        public void Dispose()
        {
            _modalDialogService.OnClose -= LoadTableDataAsync;
        }
    }
}
