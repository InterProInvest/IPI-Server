using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class ResetAuthenticatorPage : ComponentBase
    {
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<ResetAuthenticatorPage> Logger { get; set; }

        public bool Initialized { get; set; } = true;
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }

        private async Task ResetAuthenticatorKeyAsync()
        {           
            try
            {
                await BreadcrumbsService.SetResetAuthenticator();

                var response = await HttpClient.PostAsync("api/Identity/ResetAuthenticatorKey", new StringContent(string.Empty));

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                await ToastService.ShowToastAsync("Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.", ToastType.Success);
                NavigationManager.NavigateTo("/Identity/Account/Manage/EnableAuthenticator", false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }
    }
}