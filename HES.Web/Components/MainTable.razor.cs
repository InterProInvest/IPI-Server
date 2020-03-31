using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace HES.Web.Components
{
    public partial class MainTable<TItem> : ComponentBase
    {
        [Parameter] public RenderFragment TableHeader { get; set; }
        [Parameter] public RenderFragment<TItem> TableRow { get; set; }
        [Parameter] public IReadOnlyList<TItem> Items { get; set; }

        public string CurrentItemId { get; set; }

        private void OnRowSelected(TItem item)
        {
            CurrentItemId = (string)item.GetType().GetProperty("Id").GetValue(item);
        }

        private string GetRowStyle(TItem item)
        {
            return CurrentItemId == (string)item.GetType().GetProperty("Id").GetValue(item) ? "table-selected-row" : string.Empty;
        }

        private void OnRowDblClick()
        {

        }
    }
}