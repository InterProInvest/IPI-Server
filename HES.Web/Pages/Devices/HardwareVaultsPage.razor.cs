using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Devices
{
    public partial class HardwareVaultsPage : ComponentBase
    {
        [Inject]
        public IDeviceService HardwareVaultService { get; set; }

        public List<Device> HardwareVaults { get; set; }
        public Device SelectedHardwareVault { get; set; }
        
        public HardwareVaultFilter Filter { get; set; } = new HardwareVaultFilter();
        public string SearchText { get; set; } = string.Empty;
        public ListSortDirection SortDirection { get; set; } = ListSortDirection.Ascending;
        public string SortedColumn { get; set; } = nameof(Device.Id);
        public int DisplayRows { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
        public int TotalRecords { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadTableDataAsync();
        }

        private async Task LoadTableDataAsync()
        {
            var currentTotalRows = TotalRecords;
            TotalRecords = await HardwareVaultService.GetVaultsCountAsync(SearchText, Filter);

            if (currentTotalRows != TotalRecords)
                CurrentPage = 1;

            HardwareVaults = await HardwareVaultService.GetHardwareVaultsAsync((CurrentPage - 1) * DisplayRows, DisplayRows, SortedColumn, SortDirection, SearchText, Filter);
            SelectedHardwareVault = null;

            StateHasChanged();
        }


        private async Task SelectedItemChangedAsync(Device hardwareVault)
        {
            await InvokeAsync(() => 
            {
                SelectedHardwareVault = hardwareVault;
                StateHasChanged();
            });
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
    }
}
