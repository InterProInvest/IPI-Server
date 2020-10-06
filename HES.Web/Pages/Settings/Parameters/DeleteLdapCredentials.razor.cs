using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Parameters
{
    public partial class DeleteLdapCredentials : ComponentBase
    {
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public ILogger<DeleteLdapCredentials> Logger { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        private async Task DeleteAsync()
        {
            try
            {
                var ldapSettings = await AppSettingsService.GetLdapSettingsAsync();
                ldapSettings.UserName = null;
                ldapSettings.Password = null;
                await AppSettingsService.SetLdapSettingsAsync(ldapSettings);
                await ToastService.ShowToastAsync("Domain settings updated.", ToastType.Success);
                await HubContext.Clients.All.SendAsync(RefreshPage.Parameters, ConnectionId);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CloseAsync();
            }
        }
    }
}