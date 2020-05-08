using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;

namespace HES.Web.Pages.Employees
{
    public partial class HardwareVaultDetails : ComponentBase
    {
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Parameter] public HardwareVault HardwareVault { get; set; }
    }
}