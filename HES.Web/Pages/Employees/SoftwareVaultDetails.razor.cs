using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;

namespace HES.Web.Pages.Employees
{
    public partial class SoftwareVaultDetails: ComponentBase
    {
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Parameter] public SoftwareVault SoftwareVault { get; set; }  
    }
}