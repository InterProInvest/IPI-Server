using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Account;
using HES.Core.Models.Web.HardwareVaults;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Web.Pages.Employees
{
    public partial class CreateEmployee : ComponentBase
    {
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Inject] public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public ISharedAccountService SheredAccountSevice { get; set; }
        [Inject] public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public ILogger<CreateEmployee> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public List<HardwareVault> HardwareVaults { get; set; }
        public List<Company> Companies { get; set; }
        public List<Department> Departments { get; set; }
        public List<Position> Positions { get; set; }
        public List<SharedAccount> SharedAccounts { get; set; }
        public ValidationErrorMessage EmployeeValidationErrorMessage { get; set; }
        public WorkstationAccountType WorkstationType { get; set; }
        public AccountType AccountType { get; set; }

        private bool initialized = false;
        private WizardStep wizardStep = WizardStep.Profile;
        private string cssProfile = "wizard-item-process";
        private string cssHardwareVault = "wizard-item-wait";
        private string cssWorkstationAccount = "wizard-item-wait";
        private string cssOverview = "wizard-item-wait";
        private Employee employee;
        private EditContext employeeContext;
        private string hardwareVaultId;
        private WorkstationAccount workstationAccount;
        private EditContext workstationAccountContext;
        private WorkstationDomain workstationDomain;
        private EditContext workstationDomainContext;
        private bool accountSkiped;
        private string sharedAccountId;

        protected override async Task OnInitializedAsync()
        {
            await InitializeCollections();
            await InitializeModels();
            initialized = true;
        }

        private async Task InitializeCollections()
        {
            Companies = await OrgStructureService.GetCompaniesAsync();
            Departments = new List<Department>();
            Positions = await OrgStructureService.GetPositionsAsync();
            SharedAccounts = await SheredAccountSevice.GetWorkstationSharedAccountsAsync();
            sharedAccountId = SharedAccounts.FirstOrDefault()?.Id;
            var hardwareVaultsCount = await HardwareVaultService.GetVaultsCountAsync(new DataLoadingOptions<HardwareVaultFilter>() { Filter = new HardwareVaultFilter() { Status = VaultStatus.Ready } });
            HardwareVaults = await HardwareVaultService.GetVaultsAsync(new DataLoadingOptions<HardwareVaultFilter>()
            {
                Skip = 0,
                Take = hardwareVaultsCount,
                SortedColumn = nameof(HardwareVault.Id),
                SortDirection = ListSortDirection.Ascending,
                SearchText = string.Empty,
                Filter = new HardwareVaultFilter() { Status = VaultStatus.Ready }
            });
            hardwareVaultId = HardwareVaults.FirstOrDefault()?.Id;
        }

        private Task InitializeModels()
        {
            employee = new Employee() { Id = Guid.NewGuid().ToString() };
            employeeContext = new EditContext(employee);
            workstationAccount = new WorkstationAccount() { EmployeeId = employee.Id };
            workstationAccountContext = new EditContext(workstationAccount);
            workstationDomain = new WorkstationDomain() { EmployeeId = employee.Id };
            workstationDomainContext = new EditContext(workstationDomain);
            return Task.CompletedTask;
        }

        private async Task Next()
        {
            switch (wizardStep)
            {
                case WizardStep.Profile:
                    var employeeNameExist = await EmployeeService.CheckEmployeeNameExistAsync(employee);
                    if (employeeNameExist)
                    {
                        EmployeeValidationErrorMessage.DisplayError(nameof(Employee.FirstName), $"{employee.FirstName} {employee.LastName} already exists.");
                        return;
                    }
                    var employeeIsValid = employeeContext.Validate();
                    if (!employeeIsValid)
                        return;
                    cssProfile = "wizard-item-finish";
                    cssHardwareVault = "wizard-item-process";
                    wizardStep = WizardStep.HardwareVault;
                    break;
                case WizardStep.HardwareVault:
                    cssHardwareVault = "wizard-item-finish";
                    cssWorkstationAccount = "wizard-item-process";
                    wizardStep = WizardStep.WorkstationAccount;
                    break;
                case WizardStep.WorkstationAccount:
                    cssWorkstationAccount = "wizard-item-finish";
                    cssOverview = "wizard-item-process";
                    if (AccountType == AccountType.Personal)
                    {
                        if (WorkstationType == WorkstationAccountType.Domain)
                        {
                            var workstationDomainIsValid = workstationDomainContext.Validate();
                            if (!workstationDomainIsValid)
                                return;
                            workstationAccount.Type = WorkstationType;
                        }
                        else
                        {
                            var workstationAccountIsValid = workstationAccountContext.Validate();
                            if (!workstationAccountIsValid)
                                return;
                            workstationDomain.Type = WorkstationType;
                        }
                    }
                    wizardStep = WizardStep.Overview;
                    break;
                case WizardStep.Overview:
                    await CreateAsync();
                    break;
            }
        }

        private void Back()
        {
            switch (wizardStep)
            {
                case WizardStep.Profile:
                    break;
                case WizardStep.HardwareVault:
                    cssProfile = "wizard-item-process";
                    cssHardwareVault = "wizard-item-wait";
                    wizardStep = WizardStep.Profile;
                    break;
                case WizardStep.WorkstationAccount:
                    cssHardwareVault = "wizard-item-process";
                    cssWorkstationAccount = "wizard-item-wait";
                    wizardStep = WizardStep.HardwareVault;
                    break;
                case WizardStep.Overview:
                    accountSkiped = false;
                    cssWorkstationAccount = "wizard-item-process";
                    cssOverview = "wizard-item-wait";
                    wizardStep = WizardStep.WorkstationAccount;
                    break;
            }
        }

        private void SkipAccount()
        {
            cssWorkstationAccount = "wizard-item-finish";
            cssOverview = "wizard-item-process";
            accountSkiped = true;
            wizardStep = WizardStep.Overview;
        }

        private async Task CompanyChangedAsync(ChangeEventArgs args)
        {
            Departments = await OrgStructureService.GetDepartmentsByCompanyIdAsync(args.Value.ToString());
            employee.DepartmentId = Departments.FirstOrDefault()?.Id;
        }

        private void AccountTypeChanged(AccountType accountType)
        {
            AccountType = accountType;
        }

        private async Task CreateAsync()
        {
            try
            {
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await EmployeeService.CreateEmployeeAsync(employee);

                    if (hardwareVaultId != null)
                        await EmployeeService.AddHardwareVaultAsync(employee.Id, hardwareVaultId);

                    if (!accountSkiped)
                    {
                        if (AccountType == AccountType.Personal)
                        {
                            switch (WorkstationType)
                            {
                                case WorkstationAccountType.Local:
                                case WorkstationAccountType.AzureAD:
                                case WorkstationAccountType.Microsoft:
                                    await EmployeeService.CreateWorkstationAccountAsync(workstationAccount);
                                    break;
                                case WorkstationAccountType.Domain:
                                    await EmployeeService.CreateWorkstationAccountAsync(workstationDomain);
                                    break;
                            }
                        }
                        else
                        {
                            await EmployeeService.AddSharedAccountAsync(employee.Id, sharedAccountId);
                        }
                    }

                    transactionScope.Complete();
                }

                if (hardwareVaultId != null)
                    RemoteWorkstationConnectionsService.StartUpdateRemoteDevice(hardwareVaultId);

                ToastService.ShowToast("Employee created.", ToastLevel.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Employees);
                await ModalDialogService.CloseAsync();
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