using Microsoft.AspNetCore.Components;

namespace HES.Web.Components
{
    public partial class TableAction : ComponentBase
    {
        [Parameter] public RenderFragment ChildContent { get; set; }
    }
}