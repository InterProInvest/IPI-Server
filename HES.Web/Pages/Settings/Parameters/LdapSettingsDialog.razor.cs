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

        protected override async Task OnInitializedAsync()
        {
            var setting = await AppSettingsService.GetLdapSettingsAsync();

            if (setting == null)
                LdapSettings = new LdapSettings() { Host = Host };

            else
                LdapSettings = new LdapSettings()
                {
                    Host = Host,
                    MaxPasswordAge = setting.MaxPasswordAge
                };

            LdapSettingsContext = new EditContext(LdapSettings);
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                var isValid = LdapSettingsContext.Validate();

                if (!isValid)
                    return;

                await AppSettingsService.SetLdapSettingsAsync(LdapSettings);
                await ToastService.ShowToastAsync("Domain settings updated.", ToastType.Success);
                await HubContext.Clients.All.SendAsync(RefreshPage.Parameters, ConnectionId);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CloseAsync();
            }
        }
    }
}
