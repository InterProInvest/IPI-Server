using System.ComponentModel;

namespace HES.Core.Models.Web
{
    public class DataLoadingOptions<TFilter> where TFilter : class, new()
    {
        public int Take { get; set; }
        public int Skip { get; set; }
        public TFilter Filter { get; set; }
        public string SearchText { get; set; }
        public string SortedColumn { get; set; }
        public ListSortDirection SortDirection { get; set; }

        public DataLoadingOptions()
        {
            Take = 10;
            Skip = 0;
            Filter = new TFilter();
            SearchText = string.Empty;
            SortDirection = ListSortDirection.Ascending;
        }
    }
}