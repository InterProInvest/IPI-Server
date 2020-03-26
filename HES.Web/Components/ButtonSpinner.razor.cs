using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public partial class ButtonSpinner : ComponentBase
    {
        [Parameter] public string Text { get; set; } = "Button";
        [Parameter] public string Class { get; set; } = string.Empty;
        [Parameter] public EventCallback Callback { get; set; }

        private bool _isBusy { get; set; }

        private async Task ClickHandler()
        {
            if (_isBusy)
            {
                return;
            }

            _isBusy = true;

            try
            {
                await Callback.InvokeAsync(this);
            }
            finally
            {
                _isBusy = false;
            }
        }
    }
}
