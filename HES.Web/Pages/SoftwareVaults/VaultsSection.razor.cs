using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.SoftwareVaults
{
    public partial class VaultsSection : ComponentBase
    {
        [Inject] public ISoftwareVaultService SoftwareVaultService { get; set; }

        public IList<SoftwareVault> SoftwareVaults { get; set; }

        protected override async Task OnInitializedAsync()
        {
            SoftwareVaults = await SoftwareVaultService.GetSoftwareVaultsAsync();
        }
    }
}