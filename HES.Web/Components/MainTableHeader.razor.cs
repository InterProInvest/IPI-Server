using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public partial class MainTableHeader : ComponentBase
    {
        [Parameter] public string Title { get; set; }
        [Parameter] public double TitleWidth { get; set; }
        [Parameter] public string SortColumn { get; set; }
        [Parameter] public string CurrentSortedColumn { get; set; }
        [Parameter] public Func<string,Task> SortedColumnChanged { get; set; }
        [Parameter] public Func<ListSortDirection, Task> SortDirectionChanged { get; set; }

        private ListSortDirection _sortDirection = ListSortDirection.Ascending;

        private async Task SortTable()
        {
            if (CurrentSortedColumn != SortColumn)
            {
                await SortedColumnChanged.Invoke(SortColumn);
            }
            else
            {
                _sortDirection = _sortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                await SortDirectionChanged.Invoke(_sortDirection);
            }
        }

        private string GetSortIcon()
        {
            if (SortColumn != CurrentSortedColumn)
            {
                return string.Empty;
            }

            if (_sortDirection == ListSortDirection.Ascending)
            {
                return "table-sort-arrow-up";
            }
            else
            {
                return "table-sort-arrow-down";
            }
        }
    }
}