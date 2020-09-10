using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Accounts;
using HES.Core.Models.Web.AppSettings;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class CreatePersonalAccount : ComponentBase
    {
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Inject] public ITemplateService TemplateService { get; set; }
        [Inject] public ILdapService LdapService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<CreatePersonalAccount> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }
        [Parameter] public string EmployeeId { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public PersonalAccount PersonalAccount { get; set; }
        public List<Template> Templates { get; set; }
        public WorkstationAccountType WorkstationType { get; set; }
        public WorkstationAccount WorkstationAccount { get; set; }
        public WorkstationDomain WorkstationDomain { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public LdapSettings LdapSettings { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }
        public ButtonSpinner ButtonSpinnerWorkstationAccount { get; set; }

        private bool _isBusy;

        protected override async Task OnInitializedAsync()
        {
            LdapSettings = await AppSettingsService.GetLdapSettingsAsync();
            Templates = await TemplateService.GetTemplatesAsync();
            WorkstationType = WorkstationAccountType.Local;
            WorkstationAccount = new WorkstationAccount() { EmployeeId = EmployeeId };
            WorkstationDomain = new WorkstationDomain() { EmployeeId = EmployeeId };
            PersonalAccount = new PersonalAccount() { EmployeeId = EmployeeId };
        }

        private async Task CreateAccountAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await EmployeeService.CreatePersonalAccountAsync(PersonalAccount);
                    RemoteWorkstationConnectionsService.StartUpdateRemoteDevice(await EmployeeService.GetEmployeeVaultIdsAsync(EmployeeId));
                    await Refresh.InvokeAsync(this);
                    ToastService.ShowToast("Account created.", ToastLevel.Success);
                    await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.EmployeesDetails, EmployeeId);
                    await ModalDialogService.CloseAsync();
                });
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(PersonalAccount.Name), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        private async Task CreateWorkstationAccountAsync()
        {
            try
            {
                await ButtonSpinnerWorkstationAccount.SpinAsync(async () =>
                {
                    switch (WorkstationType)
                    {
                        case WorkstationAccountType.Local:
                        case WorkstationAccountType.Microsoft:
                        case WorkstationAccountType.AzureAD:
                            WorkstationAccount.Type = WorkstationType;
                            await EmployeeService.CreateWorkstationAccountAsync(WorkstationAccount);
                            break;
                        case WorkstationAccountType.Domain:
                            await EmployeeService.CreateWorkstationAccountAsync(WorkstationDomain);
                            if (WorkstationDomain.UpdateActiveDirectoryPassword)
                                await LdapService.SetUserPasswordAsync(EmployeeId, WorkstationDomain.Password, LdapSettings);
                            break;
                    }

                    RemoteWorkstationConnectionsService.StartUpdateRemoteDevice(await EmployeeService.GetEmployeeVaultIdsAsync(EmployeeId));
                    await Refresh.InvokeAsync(this);
                    ToastService.ShowToast("Account created.", ToastLevel.Success);
                    await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.EmployeesDetails, EmployeeId);
                    await ModalDialogService.CloseAsync();
                });
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(PersonalAccount.Name), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        private void TemplateSelected(ChangeEventArgs e)
        {
            var template = Templates.FirstOrDefault(x => x.Id == e.Value.ToString());
            if (template != null)
            {
                PersonalAccount.Name = template.Name;
                PersonalAccount.Urls = template.Urls;
                PersonalAccount.Apps = template.Apps;
            }
        }

        private async Task CloseAsync()
        {
            await ModalDialogService.CloseAsync();
        }
    }
}