using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public partial class DetailsLicenseOrder : ComponentBase
    {
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Parameter] public LicenseOrder LicenseOrder { get; set; }
    }
}