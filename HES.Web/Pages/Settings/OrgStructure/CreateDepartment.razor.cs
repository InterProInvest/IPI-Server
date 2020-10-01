using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.OrgStructure
{
    public partial class CreateDepartment : ComponentBase
    {
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<CreateDepartment> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string CompanyId { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public Department Department { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }

        protected override void OnInitialized()
        {
            Department = new Department() { CompanyId = CompanyId };
        }

        private async Task CreateAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await OrgStructureService.CreateDepartmentAsync(Department);
                    ToastService.ShowToast("Department created.", ToastLevel.Success);
                    await Refresh.InvokeAsync(this);
                    await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.OrgSructureCompanies);
                    await ModalDialogService.CloseAsync();
                });
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(Department.Name), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
            }
        }
    }
}
