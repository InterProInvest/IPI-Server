using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class MainTableService<TItem, TFilter> : IDisposable, IMainTableService<TItem, TFilter> where TItem : class where TFilter : class, new()
    {
        private IModalDialogService _modalDialogService;
        private Func<DataLoadingOptions<TFilter>, Task<int>> _getEntitiesCount;
        private Func<DataLoadingOptions<TFilter>, Task<List<TItem>>> _getEntities;
        private Action _stateHasChanged;

        public DataLoadingOptions<TFilter> DataLoadingOptions { get; set; }
        public TItem SelectedEntity { get; set; }
        public List<TItem> Entities { get; set; }
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; } = 1;
        public string SyncPropName { get; set; }
        public bool Loading { get; private set; }

        public MainTableService()
        {
            DataLoadingOptions = new DataLoadingOptions<TFilter>();
        }

        public async Task InitializeAsync(Func<DataLoadingOptions<TFilter>, Task<List<TItem>>> getEntities, Func<DataLoadingOptions<TFilter>, Task<int>> getEntitiesCount, IModalDialogService modalDialogService, Action stateHasChanged, string sortedColumn, ListSortDirection sortDirection = ListSortDirection.Ascending, string syncPropName = "Id", string entityId = null)
        {
            _stateHasChanged = stateHasChanged;
            _getEntities = getEntities;
            _getEntitiesCount = getEntitiesCount;
            _modalDialogService = modalDialogService;
            _modalDialogService.OnClose += LoadTableDataAsync;
            DataLoadingOptions.SortedColumn = sortedColumn;
            DataLoadingOptions.SortDirection = sortDirection;
            DataLoadingOptions.EntityId = entityId;
            SyncPropName = syncPropName;
            await LoadTableDataAsync();
        }

        public async Task LoadTableDataAsync()
        {
            if (Loading)
                return;

            try
            {
                Loading = true;
                _stateHasChanged?.Invoke();
       
                TotalRecords = await _getEntitiesCount.Invoke(DataLoadingOptions);
                SetCurrentPage();
                DataLoadingOptions.Skip = (CurrentPage - 1) * DataLoadingOptions.Take;
                Entities = await _getEntities.Invoke(DataLoadingOptions);
                SelectedEntity = Entities.FirstOrDefault(x => x.GetType().GetProperty(SyncPropName).GetValue(x).ToString() == SelectedEntity?.GetType().GetProperty(SyncPropName).GetValue(SelectedEntity).ToString());
            }
            finally
            {
                Loading = false;
                _stateHasChanged?.Invoke();
            }
        }

        public Task SelectedItemChangedAsync(TItem item)
        {
            SelectedEntity = item;
            _stateHasChanged?.Invoke();
            return Task.CompletedTask;
        }

        public async Task FilterChangedAsync(TFilter filter)
        {
            DataLoadingOptions.Filter = filter;
            await LoadTableDataAsync();
        }

        public async Task CurrentPageChangedAsync(int currentPage)
        {
            CurrentPage = currentPage;
            await LoadTableDataAsync();
        }

        public async Task DisplayRowsChangedAsync(int displayRows)
        {
            DataLoadingOptions.Take = displayRows;
            SetCurrentPage();
            await LoadTableDataAsync();
        }

        public async Task SearchTextChangedAsync(string searchText)
        {
            DataLoadingOptions.SearchText = searchText;
            await LoadTableDataAsync();
        }

        public async Task SortedColumnChangedAsync(string columnName)
        {
            DataLoadingOptions.SortedColumn = columnName;
            await LoadTableDataAsync();
        }

        public async Task SortDirectionChangedAsync(ListSortDirection sortDirection)
        {
            DataLoadingOptions.SortDirection = sortDirection;
            await LoadTableDataAsync();
        }

        public async Task ShowModalAsync(string modalTitle, RenderFragment modalBody, ModalDialogSize modalSize = ModalDialogSize.Default)
        {
            await _modalDialogService.ShowAsync(modalTitle, modalBody, modalSize);
        }

        private void SetCurrentPage()
        {
            var totalPages = (int)Math.Ceiling(TotalRecords / (decimal)DataLoadingOptions.Take);

            if (totalPages == 0)
            {
                CurrentPage = 1;
                return;
            }

            CurrentPage = totalPages < CurrentPage ? totalPages : CurrentPage;
        }

        public void Dispose()
        {
            if(_modalDialogService != null)
                _modalDialogService.OnClose -= LoadTableDataAsync;
        }
    }
}