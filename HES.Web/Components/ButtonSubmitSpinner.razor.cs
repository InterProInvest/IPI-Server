using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public partial class ButtonSubmitSpinner : ComponentBase
    {
        [Parameter] public string Text { get; set; } = "Button";
        [Parameter] public string Class { get; set; } = string.Empty;

        private bool _isBusy { get; set; }

        public void ToggleSpinner()
        {
            _isBusy = !_isBusy;
            StateHasChanged();
        }
    }
}