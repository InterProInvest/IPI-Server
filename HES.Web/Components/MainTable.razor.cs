using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public partial class MainTable<TItem> : ComponentBase
    {
        [Parameter] public RenderFragment TableHeader { get; set; }
        [Parameter] public RenderFragment<TItem> TableRow { get; set; }
        [Parameter] public IReadOnlyList<TItem> Items { get; set; }
        [Parameter] public Func<string, Task> ItemIdChanged { get; set; }
        [Parameter] public string ItemId { get; set; }
        [Parameter] public Func<Task> ItemDblClick { get; set; }


        private async Task OnRowSelected(TItem item)
        {
            ItemId = (string)item.GetType().GetProperty("Id").GetValue(item);
            await ItemIdChanged.Invoke(ItemId);
        }

        private string GetRowStyle(TItem item)
        {
            return ItemId == (string)item.GetType().GetProperty("Id").GetValue(item) ? "table-selected-row" : string.Empty;
        }

        private async Task OnRowDblClick()
        {
            await ItemDblClick.Invoke();
        }
    }
}