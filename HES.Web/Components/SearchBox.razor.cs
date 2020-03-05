using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace HES.Web.Components
{
    public partial class SearchBox : ComponentBase
    {
        [Parameter] public int Delay { get; set; } = 500;
        [Parameter] public Func<string, Task> Search { get; set; }
        [Parameter] public string Placeholder { get; set; } = "Search";

        public string SearchString { get; set; }
        private Timer _timer;

        protected override void OnInitialized()
        {
            _timer = new Timer(Delay);
            _timer.Elapsed += async (sender, args) =>
            {
                await InvokeAsync(async () =>
                {
                    await Search.Invoke(SearchString.Trim());
                    StateHasChanged();
                });
            };
            _timer.AutoReset = false;
        }

        public void OnKeyUp(KeyboardEventArgs e)
        {
            _timer.Stop();
            _timer.Start();
        }
    }
}
