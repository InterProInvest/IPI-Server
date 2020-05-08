using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Account;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class CreatePersonalAccount : ComponentBase
    {
        [Inject] IEmployeeService EmployeeService { get; set; }
        [Inject] ITemplateService TemplateService { get; set; }
        [Inject] IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] ILogger<CreatePersonalAccount> Logger { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }
        [Parameter] public string EmployeeId { get; set; }

        public PersonalAccount PersonalAccount { get; set; }
        public List<Template> Templates { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }

        private bool _isBusy;

        protected override async Task OnInitializedAsync()
        {
            Templates = await TemplateService.GetTemplatesAsync();
            PersonalAccount = new PersonalAccount() { EmployeeId = EmployeeId };
        }

        private async Task CreateAccountAsync()
        {
            try
            {
                if (_isBusy)
                    return;

                _isBusy = true;

                await EmployeeService.CreatePersonalAccountAsync(PersonalAccount);
                RemoteWorkstationConnectionsService.StartUpdateRemoteDevice(await EmployeeService.GetEmployeeVaultIdsAsync(EmployeeId));
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Account created.", ToastLevel.Success);
                await ModalDialogService.CloseAsync();
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
            finally
            {
                _isBusy = false;
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