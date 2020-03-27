using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class DetailsSoftwareVaultsInvitationSection : ComponentBase
    {
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Parameter] public string EmployeeId { get; set; }
        public Employee Employee { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Employee = await EmployeeService.GetEmployeeByIdAsync(EmployeeId);
        }
    }
}