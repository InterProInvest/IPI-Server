using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class DetailsButtonSection : ComponentBase
    {
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Parameter] public string EmployeeId { get; set; }

        public Employee Employee { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Employee = await EmployeeService.GetEmployeeByIdAsync(EmployeeId);
        }

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
