using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.LicenseOrders;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public partial class CreateLicenseOrder : ComponentBase
    {
        [Inject] ILicenseService LicenseService { get; set; }
        [Inject] IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] ILogger<CreateLicenseOrder> Logger { get; set; }

        public ValidationErrorMessage ValidationErrorMessage { get; set; }

        private NewLicenseOrder _newLicenseOrder;
        private RenewLicenseOrder _renewLicenseOrder;
        private bool _isBusy;
        private bool _initialized;

        protected override async Task OnInitializedAsync()
        {
            _newLicenseOrder = new NewLicenseOrder()
            {
                HardwareVaults = await HardwareVaultService.GetVaultsWithoutLicenseAsync()
            };

            _renewLicenseOrder = new RenewLicenseOrder()
            {
                HardwareVaults = await HardwareVaultService.GetVaultsWithLicenseAsync()
            };

            _initialized = true;
        }

        private async Task CreateNewLicenseOrderAsync()
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

                var licenseOrder = new LicenseOrder()
                {
                    ContactEmail = _newLicenseOrder.ContactEmail,
                    Note = _newLicenseOrder.Note,
                    ProlongExistingLicenses = false,
                    StartDate = _newLicenseOrder.StartDate.Date,
                    EndDate = _newLicenseOrder.EndDate.Date
                };

                var checkedHardwareVaults = _newLicenseOrder.HardwareVaults.Where(x => x.Checked).ToList();
                await LicenseService.CreateOrderAsync(licenseOrder, checkedHardwareVaults);
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

        private async Task CreateRenewLicenseOrderAsync()
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

                if (_isBusy)
                    return;

                _isBusy = true;

                var hardwareVaults = await HardwareVaultService.VaultQuery().Where(x => _renewLicenseOrder.HardwareVaults.Select(d => d.Id).Contains(x.Id)).ToListAsync();
                var maxEndDate = hardwareVaults.Select(x => x.LicenseEndDate).Max();

                if (_renewLicenseOrder.EndDate < maxEndDate)
                {
                    ValidationErrorMessage.DisplayError(nameof(RenewLicenseOrder.HardwareVaults), $"The selected End Date less than max end date for selected hardware vaults.");
                    return;
                }

                var licenseOrder = new LicenseOrder()
                {
                    ContactEmail = _renewLicenseOrder.ContactEmail,
                    Note = _renewLicenseOrder.Note,
                    ProlongExistingLicenses = true,
                    StartDate = null,
                    EndDate = _renewLicenseOrder.EndDate.Date
                };

                var checkedHardwareVaults = _renewLicenseOrder.HardwareVaults.Where(x => x.Checked).ToList();
                await LicenseService.CreateOrderAsync(licenseOrder, checkedHardwareVaults);
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
    }
}