using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public delegate Task OnClick();

    public enum ButtonStyle
    {
        Primary,
        Secondary,
        Success,
        Danger,
        Warning,
        Info,
        Light,
        Dark,
        Link
    }

    public partial class Button : ComponentBase
    {
        [Parameter] public string Text { get; set; } = "Button";

        [Parameter] public string Class { get; set; } = string.Empty;

        [Parameter] public ButtonStyle Style { get; set; } = ButtonStyle.Primary;

        [Parameter] public EventCallback OnClickAction { get; set; }

        public bool IsBusy { get; set; }

        private async Task OnClickHandler()
        {
            IsBusy = true;

            await InvokeAsync(async () => {
                await OnClickAction.InvokeAsync(this);
                StateHasChanged();
            });
            

            IsBusy = false;
        }

        private string GetButtonStyle()
        {
            var tmp =  $"btn-{Style.ToString().ToLower()}";
            return tmp;
        }
    }
}
