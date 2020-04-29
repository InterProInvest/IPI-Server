using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class VaultsSection : ComponentBase
    {
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Parameter] public string EmployeeId { get; set; }
        public Employee Employee { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await GetEmployeeAsync();
        }

        private async Task GetEmployeeAsync()
        {
            Employee = await EmployeeService.GetEmployeeByIdAsync(EmployeeId);
        }

        private async Task OpenDialogResendInvitationAsync(SoftwareVaultInvitation softwareVaultInvitation)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SoftwareVaults.ResendSoftwareVaultInvitation));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, GetEmployeeAsync));
                builder.AddAttribute(2, "SoftwareVaultInvitation", softwareVaultInvitation);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Resend invitation", body);
        }

        private async Task OpenDialogDeleteInvitationAsync(SoftwareVaultInvitation softwareVaultInvitation)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SoftwareVaults.DeleteSoftwareVaultInvitation));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, GetEmployeeAsync));
                builder.AddAttribute(2, "SoftwareVaultInvitation", softwareVaultInvitation);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete invitation", body);
        }

        private async Task OpenDialogSoftwareVaultDetailsAsync(SoftwareVault softwareVault)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SoftwareVaultDetails));
                builder.AddAttribute(1, "SoftwareVault", softwareVault);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Software vault details", body);
        }

        private async Task OpenDialogHardwareVaultDetailsAsync(HardwareVault device)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(HardwareVaultDetails));
                builder.AddAttribute(1, "Device", device);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Hardware vault details", body);
        }
    }
}