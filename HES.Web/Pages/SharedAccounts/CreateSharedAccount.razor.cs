﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web.SharedAccounts;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.SharedAccounts
{
    public partial class CreateSharedAccount : OwningComponentBase
    {
        public ISharedAccountService SharedAccountService { get; set; }
        public ITemplateService TemplateService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<CreateSharedAccount> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public SharedAccount SharedAccount { get; set; }
        public WorkstationSharedAccount WorkstationSharedAccount { get; set; }
        public WorkstationDomainSharedAccount WorkstationDomainSharedAccount { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }
        public ButtonSpinner ButtonSpinnerWorkstationAccount { get; set; }
        public WorkstationAccountType WorkstationType { get; set; }
        public List<Template> Templates { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                SharedAccountService = ScopedServices.GetRequiredService<ISharedAccountService>();
                TemplateService = ScopedServices.GetRequiredService<ITemplateService>();

                Templates = await TemplateService.GetTemplatesAsync();
                WorkstationType = WorkstationAccountType.Local;
                WorkstationSharedAccount = new WorkstationSharedAccount();
                WorkstationDomainSharedAccount = new WorkstationDomainSharedAccount();
                SharedAccount = new SharedAccount();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message); 
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task CreateAccountAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await SharedAccountService.CreateSharedAccountAsync(SharedAccount);
                    await ToastService.ShowToastAsync("Account created.", ToastType.Success);
                    await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.SharedAccounts);
                    await ModalDialogService.CloseAsync();
                });
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
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
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
                            WorkstationSharedAccount.Type = WorkstationType;
                            await SharedAccountService.CreateWorkstationSharedAccountAsync(WorkstationSharedAccount);
                            break;
                        case WorkstationAccountType.Domain:
                            await SharedAccountService.CreateWorkstationSharedAccountAsync(WorkstationDomainSharedAccount);
                            break;
                    }

                    await ToastService.ShowToastAsync("Account created.", ToastType.Success);
                    await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.SharedAccounts);
                    await ModalDialogService.CloseAsync();
                });
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(WorkstationSharedAccount.Name), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
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
