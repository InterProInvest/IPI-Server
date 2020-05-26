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
        [Parameter] public TItem SelectedItem { get; set; }
        [Parameter] public Func<TItem, Task> SelecedItemChanged { get; set; }
        [Parameter] public Func<Task> SelecedItemDblClick { get; set; }

        private async Task OnRowSelectedAsync(TItem item)
        {
            SelectedItem = item;
            await SelecedItemChanged.Invoke(SelectedItem);
        }

        private string GetRowStyle(TItem item)
        {
            if (SelectedItem == null)
                return string.Empty;

            return SelectedItem.Equals(item) ? "table-selected-row" : string.Empty;
        }

        private async Task OnRowDblClickAsync()
        {
            if (SelecedItemDblClick == null)
                return;

            await SelecedItemDblClick.Invoke();
        }
    }
}