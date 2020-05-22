using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IMainTableService<TItem, TFilter>
    {
        public TFilter Filter { get; set; }
        public string SearchText { get; set; }
        public ListSortDirection SortDirection { get; set; }
        public string SortedColumn { get; set; }
        public int DisplayRows { get; set; }
        public int CurrentPage { get; set; }
        public int TotalRecords { get; set; }
        public TItem SelectedEntity { get; set; }
        public List<TItem> Entities { get; set; }
        void Initialize(Func<int, int, string, ListSortDirection, string, TFilter, Task<List<TItem>>> getEntities, Func<string, TFilter, Task<int>> getEntitiesCount, Action stateHasChanged, string sortedColumn);
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