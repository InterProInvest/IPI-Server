using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.LicenseOrders;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public partial class LicenseOrdersPage : ComponentBase
    {
        [Inject] public IMainTableService<LicenseOrder, LicenseOrderFilter> MainTableService { get; set; }

        [Inject] public ILicenseService LicenseService { get; set; }


        protected override async Task OnInitializedAsync()
        {
            await MainTableService.InitializeAsync(LicenseService.GetLicenseOrdersAsync, LicenseService.GetLicenseOrdersCountAsync, StateHasChanged, nameof(LicenseOrder.CreatedAt));
        }

        private async Task CreateLicenseOrderAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateOrder));
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Create Order", body, ModalDialogSize.Large);
        }

        private async Task SendLicenseOrderAsync()
        {
            await Task.CompletedTask;
        }

        private async Task LicenseOrderDetailsAsync()
        {
            await Task.CompletedTask;
        }

        private async Task EditLicenseOrderAsync()
        {
            await Task.CompletedTask;
        }

        private async Task DeleteLicenseOrderAsync()
        {
            await Task.CompletedTask;
        }
    }
}
