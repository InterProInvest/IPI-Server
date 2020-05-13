using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Parameters
{
    public partial class LdapCredentials : ComponentBase
    {
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<LdapCredentials> Logger { get; set; }
        [Parameter] public string Host { get; set; }

        private LdapSettings _ldapSettings;
        private bool _IsBusy;

        protected override void OnInitialized()
        {
            _ldapSettings = new LdapSettings() { Host = Host };
        }

        private async Task SaveAsync()
        {
            try
            {
                if (_IsBusy)
                    return;

                _IsBusy = true;

                await AppSettingsService.SetLdapSettingsAsync(_ldapSettings);
                ToastService.ShowToast("Domain settings updated.", ToastLevel.Success);
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
                _IsBusy = false;
            }
        }
    }
}