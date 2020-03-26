using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class SoftwareVaultsSection : ComponentBase
    {
        [Inject] public ILogger<SoftwareVaultsSection> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Parameter] public Employee Employee { get; set; }

        public async Task OpenModalAddSoftwareVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddSoftwareVault));
                builder.AddAttribute(1, "Employee", Employee);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add software vault", body);
        }
    }
}
