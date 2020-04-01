using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.SoftwareVault;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.SoftwareVaults
{
    public partial class SoftwareVaultInvitationsSection : ComponentBase
    {
        [Inject] public ISoftwareVaultService SoftwareVaultService { get; set; }

        public List<SoftwareVaultInvitation> SoftwareVaultInvitations { get; set; }

        #region MainTable

        public SoftwareVaultInvitationFilter Filter { get; set; } = new SoftwareVaultInvitationFilter();
        public string SearchText { get; set; } = string.Empty;
        public string SortedColumn { get; set; } = nameof(SoftwareVaultInvitation.Id);
        public ListSortDirection SortDirection { get; set; } = ListSortDirection.Ascending;
        public int DisplayRows { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
        public int TotalRecords { get; set; }
        public string CurrentInvitationId { get; set; }

        #endregion

        protected override async Task OnInitializedAsync()
        {
            await LoadTableDataAsync();
        }

        #region MainTable

        private async Task LoadTableDataAsync()
        {
            var currentTotalRows = TotalRecords;
            TotalRecords = await SoftwareVaultService.GetInvitationsCountAsync(SearchText, Filter);

            if (currentTotalRows != TotalRecords)
                CurrentPage = 1;

            SoftwareVaultInvitations = await SoftwareVaultService.GetSoftwareVaultInvitationsAsync((CurrentPage - 1) * DisplayRows, DisplayRows, SortedColumn, SortDirection, SearchText, Filter);
            CurrentInvitationId = null;

            StateHasChanged();
        }

        private async Task FilterChangedAsync(SoftwareVaultInvitationFilter filter)
        {
            Filter = filter;
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

        private Task CurrentItemIdChangedAsync(string itemId)
        {
            CurrentInvitationId = itemId;
            return Task.CompletedTask;
        }

        private Task CurrentItemDblClickAsync()
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
