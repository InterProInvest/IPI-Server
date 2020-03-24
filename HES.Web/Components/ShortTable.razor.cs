using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public partial class ShortTable<TItem> : ComponentBase
    {
        public class ItemWrapper
        {
            public string Id { get; set; }
            public TItem Item { get; set; }
            public bool Checked { get; set; }
        }

        [Parameter] public List<TItem> Items { get; set; }
        [Parameter] public RenderFragment TableHeader { get; set; }
        [Parameter] public RenderFragment<TItem> TableRow { get; set; }
        [Parameter] public Func<List<TItem>, Task> CollectionChanged { get; set; }

        private List<ItemWrapper> WrapedItems { get; set; }

        protected override void OnParametersSet()
        {
            WrapedItems = Items.Select(x => new ItemWrapper() { Id = Guid.NewGuid().ToString(), Item = x }).ToList();
        }

        private async Task OnCheckAsync(ItemWrapper item)
        {
            item.Checked = !item.Checked;
            await CollectionChanged.Invoke(WrapedItems.Where(x => x.Checked).Select(x => x.Item).ToList());
        }   

        private async Task OnCheckAllAsync(ChangeEventArgs args)
        {
            WrapedItems.ForEach(x => x.Checked = (bool)args.Value);
            await CollectionChanged.Invoke(WrapedItems.Where(x => x.Checked).Select(x => x.Item).ToList());
        }
    }
}