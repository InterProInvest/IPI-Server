using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppUsers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class Disable2faPage : ComponentBase
    {
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<Disable2faPage> Logger { get; set; }

        public TwoFactorInfo TwoFactorInfo { get; set; }
        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await BreadcrumbsService.SetDisable2fa();

                var response = await HttpClient.GetAsync("api/Identity/GetTwoFactorInfo");

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                TwoFactorInfo = JsonConvert.DeserializeObject<TwoFactorInfo>(await response.Content.ReadAsStringAsync());

                if (!TwoFactorInfo.Is2faEnabled)
                {
                    await ToastService.ShowToastAsync($"Cannot disable 2FA for user as it's not currently enabled.", ToastType.Error);
                    NavigationManager.NavigateTo("/Identity/Account/Manage/TwoFactorAuthentication", false);
                    return;
                }

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
                LoadFailed = true;
            }
        }

        private async Task DisableTwoFactorAsync()
        {
            try
            {
                var response = await HttpClient.PostAsync("api/Identity/DisableTwoFactor", new StringContent(string.Empty));

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                await ToastService.ShowToastAsync("2fa has been disabled. You can reenable 2fa when you setup an authenticator app", ToastType.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
            finally
            {
                NavigationManager.NavigateTo("/Identity/Account/Manage/TwoFactorAuthentication", false);
            }
        }
    }
}