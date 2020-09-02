using Microsoft.AspNetCore.Components;

namespace HES.Web.Components
{
    public partial class TableAction : ComponentBase
    {
        [Parameter] public RenderFragment ActionButtons { get; set; }
        [Parameter] public RenderFragment FilterButtons { get; set; }
        [Parameter] public RenderFragment FilterForm { get; set; }
    }
}