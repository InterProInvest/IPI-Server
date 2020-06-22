using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web.SharedAccounts;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.SharedAccounts
{
    public partial class CreateSharedAccount : ComponentBase
    {
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public ISharedAccountService SharedAccountService { get; set; }
        [Inject] public ITemplateService TemplateService { get; set; }
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<CreateSharedAccount> Logger { get; set; }
        [Inject] public IHubContext<SharedAccountsHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public SharedAccount SharedAccount { get; set; }
        public WorkstationSharedAccount WorkstationSharedAccount { get; set; }
        public WorkstationDomainSharedAccount WorkstationDomainSharedAccount { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public WorkstationAccountType WorkstationType { get; set; }
        public List<Template> Templates { get; set; }

        private bool _isBusy;

        protected override async Task OnInitializedAsync()
        {
            Templates = await TemplateService.GetTemplatesAsync();
            WorkstationType = WorkstationAccountType.Local;
            WorkstationSharedAccount = new WorkstationSharedAccount();
            WorkstationDomainSharedAccount = new WorkstationDomainSharedAccount();
            SharedAccount = new SharedAccount();
        }

        private async Task CreateAccountAsync()
        {
            try
            {
                if (_isBusy)
                    return;

                _isBusy = true;

                await SharedAccountService.CreateSharedAccountAsync(SharedAccount);
                ToastService.ShowToast("Account created.", ToastLevel.Success);
                await HubContext.Clients.All.SendAsync("PageUpdated", ConnectionId);
                await ModalDialogService.CloseAsync();
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(SharedAccount.Name), ex.Message);
            }
            catch (IncorrectUrlException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(SharedAccount.Urls), ex.Message);
            }
            catch (IncorrectOtpException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(SharedAccount.OtpSecret), ex.Message);
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

        private async Task CreateWorkstationAccountAsync()
        {
            try
            {
                if (_isBusy)
                    return;

                _isBusy = true;

                switch (WorkstationType)
                {
                    case WorkstationAccountType.Local:
                    case WorkstationAccountType.Microsoft:
                    case WorkstationAccountType.AzureAD:
                        WorkstationSharedAccount.Type = WorkstationType;
                        await SharedAccountService.CreateWorkstationSharedAccountAsync(WorkstationSharedAccount);
                        break;
                    case WorkstationAccountType.Domain:
                        await SharedAccountService.CreateWorkstationSharedAccountAsync(WorkstationSharedAccount);
                        break;
                }

                ToastService.ShowToast("Account created.", ToastLevel.Success);
                await HubContext.Clients.All.SendAsync("PageUpdated", ConnectionId);
                await ModalDialogService.CloseAsync();
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(WorkstationSharedAccount.Name), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CancelAsync();
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
                SharedAccount.Name = template.Name;
                SharedAccount.Urls = template.Urls;
                SharedAccount.Apps = template.Apps;
            }
        }
    }
}
