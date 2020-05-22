using System.ComponentModel;

namespace HES.Core.Models.Web
{
    public class EntitiesArgs<TFilter>
    {
        public int Take { get; set; }
        public int Skip { get; set; }
        public string SearchText { get; set; }
        public string SortedColumn { get; set; }
        public ListSortDirection SortDirection { get; set; }
        public TFilter Filter { get; set; }
    }
}