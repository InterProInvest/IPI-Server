using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Dashboard
{
    public partial class HardwareVaultTasks : ComponentBase
    {
        [Inject] public IHardwareVaultTaskService HardwareVaultTaskService { get; set; }

        public List<HardwareVaultTask> HardwareVaultTaskList { get; set; }

        protected override async Task OnInitializedAsync()
        {
            HardwareVaultTaskList = await HardwareVaultTaskService
                .TaskQuery()
                .Include(d => d.Account.Employee.Department.Company)
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}