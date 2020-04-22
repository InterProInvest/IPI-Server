using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.HardwareVault;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HES.Web.Pages.Devices
{
    public partial class HardwareVaultsPage : ComponentBase
    {
        #region DependencyInjections

        [Inject]
        IJSRuntime JSRuntime { get; set; }

        [Inject]
        IToastService ToastService { get; set; }

        [Inject]
        IDeviceService DeviceService { get; set; }

        [Inject]
        ILogger<HardwareVaultsPage> Logger { get; set; }

        [Inject]
        public IEmployeeService EmployeeService { get; set; }

        [Inject]
        public IDeviceService HardwareVaultService { get; set; }

        [Inject]
        public IOrgStructureService OrgStructureService { get; set; }

        #endregion

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


        public SelectList FilterFirmwares { get; set; }
        public SelectList FilterLicenseStatuses { get; set; }
        public SelectList FilterEmployees { get; set; }
        public SelectList FilterCompanies { get; set; }

        public async Task InitializeFilterAsync()
        {
            FilterFirmwares = new SelectList(HardwareVaults.Select(s => s.Firmware).Distinct().OrderBy(f => f).ToDictionary(t => t, t => t), "Key", "Value");
            FilterLicenseStatuses = new SelectList(Enum.GetValues(typeof(LicenseStatus)).Cast<LicenseStatus>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            FilterEmployees = new SelectList(await EmployeeService.EmployeeQuery().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(), "Id", "FullName");
            FilterCompanies = new SelectList(await OrgStructureService.CompanyQuery().ToListAsync(), "Id", "Name");
        }


        private async Task LoadTableDataAsync()
        {
            var currentTotalRows = TotalRecords;
            TotalRecords = await HardwareVaultService.GetVaultsCountAsync(SearchText, Filter);

            if (currentTotalRows != TotalRecords)
                CurrentPage = 1;

            HardwareVaults = await HardwareVaultService.GetVaultsAsync((CurrentPage - 1) * DisplayRows, DisplayRows, SortedColumn, SortDirection, SearchText, Filter);
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

        private async Task ClearAsync()
        {
            Filter = new HardwareVaultFilter();
            await LoadTableDataAsync();
        }

        #region TableActions

        public async Task SynchronizeDevicesAsync()
        {
            await JSRuntime.InvokeVoidAsync("showSpinner", "syncBtnSpinner");

            try
            {
                await DeviceService.ImportDevicesAsync();
                await LoadTableDataAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast("Somethings went wrong.", ToastLevel.Error);
            }
            finally
            {
                await JSRuntime.InvokeVoidAsync("showSpinner", "syncBtnSpinner");
            }
        }

        #endregion
    }
}
