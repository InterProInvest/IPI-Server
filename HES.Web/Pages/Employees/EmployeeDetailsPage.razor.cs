using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Breadcrumb;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class EmployeeDetailsPage : ComponentBase
    {
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Inject] public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Parameter] public string EmployeeId { get; set; }

        public Employee Employee { get; set; }
        public List<Account> Accounts { get; set; }
        public Account SelectedAccount { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await GetEmployeeAsync();   
            await LoadTableDataAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var list = new List<Breadcrumb>() {
                        new Breadcrumb () { Link = "/Employees", Content = "Employees" },
                        new Breadcrumb () { Active = true, Content = Employee.FullName }
                };

                await JSRuntime.InvokeVoidAsync("createBreadcrumbs", list);
            }
        }

        private async Task GetEmployeeAsync()
        {
            Employee = await EmployeeService.GetEmployeeByIdAsync(EmployeeId);
        }

        #region Main Table

        public int CurrentPage { get; set; } = 1;
        public int DisplayRows { get; set; } = 10;
        public int TotalRecords { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public string SortedColumn { get; set; } = nameof(Account.Name);
        public ListSortDirection SortDirection { get; set; } = ListSortDirection.Ascending;

        private async Task LoadTableDataAsync()
        {
            var currentTotalRows = TotalRecords;
            TotalRecords = await EmployeeService.GetAccountsCountAsync(SearchText, EmployeeId);

            if (currentTotalRows != TotalRecords)
                CurrentPage = 1;

            Accounts = await EmployeeService.GetAccountsAsync((CurrentPage - 1) * DisplayRows, DisplayRows, SortedColumn, SortDirection, SearchText, EmployeeId);
            SelectedAccount = null;

            StateHasChanged();
        }

        private async Task SelectedItemChangedAsync(Account account)
        {
            await InvokeAsync(() =>
            {
                SelectedAccount = account;
                StateHasChanged();
            });
        }

        private async Task CurrentPageChangedAsync(int currentPage)
        {
            CurrentPage = currentPage;
            await LoadTableDataAsync();
        }

        private async Task DisplayRowsChangedAsync(int displayRows)
        {
            DisplayRows = displayRows;
            CurrentPage = 1;
            await LoadTableDataAsync();
        }

        private async Task SearchTextChangedAsync(string searchText)
        {
            SearchText = searchText;
            await LoadTableDataAsync();
        }

        private async Task SortedColumnChangedAsync(string columnName)
        {
            SortedColumn = columnName;
            await LoadTableDataAsync();
        }

        private async Task SortDirectionChangedAsync(ListSortDirection sortDirection)
        {
            SortDirection = sortDirection;
            await LoadTableDataAsync();
        }

        #endregion

        private async Task OpenDialogAddHardwareVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddHardwareVault));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.AddAttribute(2, "EmployeeId", EmployeeId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add hardware vault", body);
        }

        public async Task OpenModalAddSoftwareVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddSoftwareVault));
                builder.AddAttribute(1, "Employee", Employee);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add software vault", body);
        }

        private async Task OpenDialogCreatePersonalAccountAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreatePersonalAccount));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.AddAttribute(2, "EmployeeId", EmployeeId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Create personal account", body, ModalDialogSize.Large);
        }

        private async Task OpenDialogAddSharedAccountAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddSharedAccount));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.AddAttribute(2, "EmployeeId", EmployeeId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add shared account", body, ModalDialogSize.Large);
        }

        private async Task OpenDialogSetAsWorkstationAccountAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SetAsWorkstationAccount));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.AddAttribute(2, "Account", SelectedAccount);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Set as workstation account", body);
        }

        private async Task OpenDialogEditPersonalAccountAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditPersonalAccount));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.AddAttribute(2, "Account", SelectedAccount);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Edit account", body);
        }

        private async Task OpenDialogEditPersonalAccountPasswordAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditPersonalAccountPwd));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.AddAttribute(2, "Account", SelectedAccount);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Edit account password", body);
        }

        private async Task OpenDialogEditPersonalAccountOtpAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditPersonalAccountOtp));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.AddAttribute(2, "Account", SelectedAccount);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Edit account otp", body);
        }

        private async Task OpenDialogDeleteAccountAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteAccount));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.AddAttribute(2, "Account", SelectedAccount);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete Account", body);
        }

        private async Task OpenDialogRemoveHardwareVaultAsync(HardwareVault hardwareVault)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteHardwareVault));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
                builder.AddAttribute(2, "HardwareVault", hardwareVault);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete hardware vault", body);
        }

        private async Task OpenDialogResendInvitationAsync(SoftwareVaultInvitation softwareVaultInvitation)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SoftwareVaults.ResendSoftwareVaultInvitation));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, GetEmployeeAsync));
                builder.AddAttribute(2, "SoftwareVaultInvitation", softwareVaultInvitation);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Resend invitation", body);
        }

        private async Task OpenDialogDeleteInvitationAsync(SoftwareVaultInvitation softwareVaultInvitation)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SoftwareVaults.DeleteSoftwareVaultInvitation));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, GetEmployeeAsync));
                builder.AddAttribute(2, "SoftwareVaultInvitation", softwareVaultInvitation);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete invitation", body);
        }

        private async Task OpenDialogSoftwareVaultDetailsAsync(SoftwareVault softwareVault)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SoftwareVaultDetails));
                builder.AddAttribute(1, "SoftwareVault", softwareVault);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Software vault details", body);
        }

        private async Task OpenDialogHardwareVaultDetailsAsync(HardwareVault hardwareVault)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(HardwareVaultDetails));
                builder.AddAttribute(1, "HardwareVault", hardwareVault);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Hardware vault details", body);
        }
    }
}