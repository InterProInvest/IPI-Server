using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public partial class ButtonSpinner : ComponentBase
    {
        [Parameter] public RenderFragment Image { get; set; }
        [Parameter] public EventCallback Callback { get; set; }
        [Parameter] public string Text { get; set; } = "Button";
        [Parameter] public string Class { get; set; } = string.Empty;
        [Parameter] public bool Submit { get; set; } = false;
        [Parameter] public bool Disabled { get; set; } = false;

        private bool _busy;

        private async Task ClickHandler()
        {
            if (_busy)
                return;

            _busy = true;

            try
            {
                await Callback.InvokeAsync(this);
            }
            finally
            {
                _busy = false;
            }
        }

        public async Task SpinAsync(Func<Task> func)
        {
            if (_busy)
                return;

            _busy = true;
            StateHasChanged();

            try
            {
                await func.Invoke();
            }
            finally
            {
                _busy = false;
                StateHasChanged();
            }
        }
    }
}