﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web.LicenseOrders;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public partial class EditLicenseOrder : ComponentBase, IDisposable
    {
        [Inject] public ILicenseService LicenseService { get; set; }
        [Inject] public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<EditLicenseOrder> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public LicenseOrder LicenseOrder { get; set; }

        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public bool EntityBeingEdited { get; set; }

        private NewLicenseOrder _newLicenseOrder;
        private RenewLicenseOrder _renewLicenseOrder;
        private bool _isBusy;
        private bool _initialized;

        protected override async Task OnInitializedAsync()
        {
            EntityBeingEdited = MemoryCache.TryGetValue(LicenseOrder.Id, out object _);
            if (!EntityBeingEdited)
                MemoryCache.Set(LicenseOrder.Id, LicenseOrder);

            if (!LicenseOrder.ProlongExistingLicenses)
            {          
                _newLicenseOrder = new NewLicenseOrder()
                {
                    ContactEmail = LicenseOrder.ContactEmail,
                    Note = LicenseOrder.Note,
                    StartDate = LicenseOrder.StartDate.Value,
                    EndDate = LicenseOrder.EndDate,
                    HardwareVaults = await HardwareVaultService.GetVaultsWithoutLicenseAsync()
                };
                _newLicenseOrder.HardwareVaults.ForEach(x => x.Checked = LicenseOrder.HardwareVaultLicenses.Any(d => d.HardwareVaultId == x.Id));
            }
            else
            {
                _renewLicenseOrder = new RenewLicenseOrder()
                {
                    ContactEmail = LicenseOrder.ContactEmail,
                    Note = LicenseOrder.Note,
                    EndDate = LicenseOrder.EndDate,
                    HardwareVaults = await HardwareVaultService.GetVaultsWithLicenseAsync()
                };
                _renewLicenseOrder.HardwareVaults.ForEach(x => x.Checked = LicenseOrder.HardwareVaultLicenses.Any(d => d.HardwareVaultId == x.Id));
            }

            _initialized = true;
        }

        private async Task EditNewLicenseOrderAsync()
        {
            try
            {
                if (_newLicenseOrder.StartDate < DateTime.Now.Date)
                {
                    ValidationErrorMessage.DisplayError(nameof(NewLicenseOrder.StartDate), $"Start Date must be at least current date.");
                    return;
                }

                if (_newLicenseOrder.EndDate < _newLicenseOrder.StartDate)
                {
                    ValidationErrorMessage.DisplayError(nameof(NewLicenseOrder.EndDate), $"End Date must not be less than Start Date.");
                    return;
                }

                if (!_newLicenseOrder.HardwareVaults.Where(x => x.Checked).Any())
                {
                    ValidationErrorMessage.DisplayError(nameof(NewLicenseOrder.HardwareVaults), $"Select at least one hardware vault.");
                    return;
                }

                if (_isBusy)
                    return;

                _isBusy = true;

                LicenseOrder.ContactEmail = _newLicenseOrder.ContactEmail;
                LicenseOrder.Note = _newLicenseOrder.Note;
                LicenseOrder.ProlongExistingLicenses = false;
                LicenseOrder.StartDate = _newLicenseOrder.StartDate.Date;
                LicenseOrder.EndDate = _newLicenseOrder.EndDate.Date;

                var checkedHardwareVaults = _newLicenseOrder.HardwareVaults.Where(x => x.Checked).ToList();
                await LicenseService.EditOrderAsync(LicenseOrder, checkedHardwareVaults);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Licenses, LicenseOrder.Id);
                ToastService.ShowToast("Order created.", ToastLevel.Success);
                await ModalDialogService.CloseAsync();
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

        private async Task EditRenewLicenseOrderAsync()
        {
            try
            {
                if (_renewLicenseOrder.EndDate < DateTime.Now)
                {
                    ValidationErrorMessage.DisplayError(nameof(RenewLicenseOrder.EndDate), $"End Date must not be less than Start Date.");
                    return;
                }

                if (!_renewLicenseOrder.HardwareVaults.Where(x => x.Checked).Any())
                {
                    ValidationErrorMessage.DisplayError(nameof(RenewLicenseOrder.HardwareVaults), $"Select at least one hardware vault.");
                    return;
                }

                var checkedHardwareVaults = _renewLicenseOrder.HardwareVaults.Where(x => x.Checked).ToList();
                var maxEndDate = checkedHardwareVaults.Select(x => x.LicenseEndDate).Max();

                if (_renewLicenseOrder.EndDate < maxEndDate)
                {
                    ValidationErrorMessage.DisplayError(nameof(RenewLicenseOrder.HardwareVaults), $"The selected End Date less than max end date for selected hardware vaults.");
                    return;
                }

                if (_isBusy)
                    return;

                _isBusy = true;

                LicenseOrder.ContactEmail = _renewLicenseOrder.ContactEmail;
                LicenseOrder.Note = _renewLicenseOrder.Note;
                LicenseOrder.ProlongExistingLicenses = true;
                LicenseOrder.StartDate = null;
                LicenseOrder.EndDate = _renewLicenseOrder.EndDate.Date;

                await LicenseService.EditOrderAsync(LicenseOrder, checkedHardwareVaults);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Licenses, LicenseOrder.Id);
                ToastService.ShowToast("Order created.", ToastLevel.Success);
                await ModalDialogService.CloseAsync();
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

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(LicenseOrder.Id);
        }
    }
}