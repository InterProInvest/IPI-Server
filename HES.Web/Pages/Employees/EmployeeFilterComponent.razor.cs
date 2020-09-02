using HES.Core.Interfaces;
using HES.Core.Models.Employees;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class EmployeeFilterComponent : ComponentBase
    {
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Parameter] public Func<EmployeeFilter, Task> FilterChanged { get; set; }

        public EmployeeFilter Filter { get; set; }

        protected override void OnInitialized()
        {
            Filter = new EmployeeFilter();
        }

        private async Task FilteredAsync()
        {
            await FilterChanged.Invoke(Filter);
        }

        private async Task ClearAsync()
        {
            Filter = new EmployeeFilter();
            await FilterChanged.Invoke(Filter);
        }
    }
}
