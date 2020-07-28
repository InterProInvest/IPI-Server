using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Parameters
{
    public partial class LdapSettingsDialog : ComponentBase
    {
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public ILogger<LdapSettingsDialog> Logger { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string Host { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public LdapSettings LdapSettings { get; set; }
        public EditContext LdapSettingsContext { get; set; }
        public bool SkipCredentials { get; set; }

        protected override void OnInitialized()
        {
            LdapSettings = new LdapSettings() { Host = Host };
            LdapSettingsContext = new EditContext(LdapSettings);
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                var isValid = LdapSettingsContext.Validate();
                var hostEmpty = string.IsNullOrWhiteSpace(LdapSettings.Host);

                if (SkipCredentials)
                {
                    LdapSettings.UserName = null;
                    LdapSettings.Password = null;

                    if (!isValid && hostEmpty)
                        return;
                }
                else
                {            
                    if (!isValid)
                        return;
                }

                await AppSettingsService.SetLdapSettingsAsync(LdapSettings);
                ToastService.ShowToast("Domain settings updated.", ToastLevel.Success);
                await HubContext.Clients.All.SendAsync(RefreshPage.Parameters, ConnectionId);
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
