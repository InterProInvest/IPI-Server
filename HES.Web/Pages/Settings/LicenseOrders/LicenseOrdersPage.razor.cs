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
        [Inject] public ILicenseService LicenseService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IMainTableService<LicenseOrder, LicenseOrderFilter> MainTableService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await MainTableService.InitializeAsync(LicenseService.GetLicenseOrdersAsync, LicenseService.GetLicenseOrdersCountAsync, StateHasChanged, nameof(LicenseOrder.CreatedAt));
            await BreadcrumbsService.SetLicenseOrders();
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
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SendLicenseOrder));
                builder.AddAttribute(1, nameof(SendLicenseOrder.LicenseOrder), MainTableService.SelectedEntity);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Send license order", body);
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
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteLicenseOrder));
                builder.AddAttribute(1, nameof(DeleteLicenseOrder.LicenseOrder), MainTableService.SelectedEntity);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Delete license order", body);
        }
    }
}
