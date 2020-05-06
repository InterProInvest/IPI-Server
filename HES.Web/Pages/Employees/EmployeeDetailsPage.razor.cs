using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class EmployeeDetailsPage : ComponentBase
    {
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Parameter] public string EmployeeId { get; set; }

        public Employee Employee { get; set; }
        public List<Account> Accounts { get; set; }
        public Account SelectedAccount { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Employee = await EmployeeService.GetEmployeeByIdAsync(EmployeeId);
            InitializeCommponents();
            await LoadTableDataAsync();
        }

        
        #region Main Table

        public int CurrentPage { get; set; }
        public int DisplayRows { get; set; }
        public int TotalRecords { get; set; }
        public string SearchText { get; set; }
        public string SortedColumn { get; set; }
        public ListSortDirection SortDirection { get; set; }

        private void InitializeCommponents()
        {
            CurrentPage = 1;
            DisplayRows = 10;
            SearchText = string.Empty;
            SortedColumn = nameof(Account.Name);
            SortDirection = ListSortDirection.Ascending;
        }

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

        private async Task SelectedItemChangedAsync(Account employee)
        {
            await InvokeAsync(() =>
            {
                SelectedAccount = employee;
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

        private async Task AddDeviceAsync()
        {
            await Task.CompletedTask;
        }

        private async Task CreatePersonalAccountAsync()
        {
            await Task.CompletedTask;
        }

        private async Task AddSharedAccountAsync()
        {
            await Task.CompletedTask;
        }

        private async Task RemoveHardwareVaultAsync()
        {
            await Task.CompletedTask;
        }
    }
}
