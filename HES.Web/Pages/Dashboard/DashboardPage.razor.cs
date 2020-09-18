using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Dashboard;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Dashboard
{
    public partial class DashboardPage : OwningComponentBase, IDisposable
    {
        public IDashboardService DashboardService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<DashboardPage> Logger { get; set; }

        public DashboardCard ServerdCard { get; set; }
        public DashboardCard EmployeesCard { get; set; }
        public DashboardCard HardwareVaultsCard { get; set; }
        public DashboardCard WorkstationsCard { get; set; }
        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }


        protected override async Task OnInitializedAsync()
        {
            try
            {
                DashboardService = ScopedServices.GetRequiredService<IDashboardService>();

                await BreadcrumbsService.SetDashboard();
                ServerdCard = await DashboardService.GetServerCardAsync();
                ServerdCard.RightAction = ShowHardwareVaultTaskAsync;
                if (ServerdCard.Notifications.FirstOrDefault(x => x.Page == "long-pending-tasks") != null)
                    ServerdCard.Notifications.FirstOrDefault(x => x.Page == "long-pending-tasks").Action = ShowHardwareVaultTaskAsync;
                EmployeesCard = await DashboardService.GetEmployeesCardAsync();
                HardwareVaultsCard = await DashboardService.GetHardwareVaultsCardAsync();
                WorkstationsCard = await DashboardService.GetWorkstationsCardAsync();

                Initialized = true;
            }
            catch (Exception ex)
            {
                LoadFailed = true;
                ErrorMessage = ex.Message;
                Logger.LogError(ex.Message);
            }
        }

        private async Task ShowHardwareVaultTaskAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(HardwareVaultTasks));
                builder.CloseComponent();
            };
            await ModalDialogService.ShowAsync("Hardware Vault Tasks", body, ModalDialogSize.Large);
        }

        public void Dispose()
        {

        }
    }
}
