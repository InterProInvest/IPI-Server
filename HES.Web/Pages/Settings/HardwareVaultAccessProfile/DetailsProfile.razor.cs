using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;

namespace HES.Web.Pages.Settings.HardwareVaultAccessProfile
{
    public partial class DetailsProfile : ComponentBase
    {
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Parameter] public HardwareVaultProfile AccessProfile { get; set; }
    }
}
