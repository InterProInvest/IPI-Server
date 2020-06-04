using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Dashboard
{
    public partial class HardwareVaultTasks : ComponentBase
    {
        [Inject] public IHardwareVaultTaskService HardwareVaultTaskService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }

        public List<HardwareVaultTask> HardwareVaultTaskList { get; set; }

        protected override async Task OnInitializedAsync()
        {
            HardwareVaultTaskList = await HardwareVaultTaskService.GetHardwareVaultTasksAsync();
        }
    }
}