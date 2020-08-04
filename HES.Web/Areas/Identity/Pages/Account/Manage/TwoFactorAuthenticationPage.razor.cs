using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Accounts;
using HES.Core.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class TwoFactorAuthenticationPage : ComponentBase
    {
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IHttpClientFactory HttpClientFactory { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<TwoFactorAuthenticationPage> Logger { get; set; }

        public TwoFactInformation TwoFactInfo { get; set; }
        public ApplicationUser AppUser { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            AppUser = await UserManager.GetUserAsync(state.User);

            HttpClient httpClient = HttpClientFactory.CreateClient();
            HttpHelper httpHelper = new HttpHelper();

            try
            {
                httpClient.BaseAddress = new Uri(NavigationManager.BaseUri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await httpClient.GetAsync($"api/Identity/GetTwoFactInformation?userId={AppUser.Id}");
                TwoFactInfo = await httpHelper.GetObjectFromResponseAsync<TwoFactInformation>(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
            finally
            {
                httpClient.Dispose();
            }
        }

        private async Task ForgetBrowserAsync()
        {
            HttpClient httpClient = HttpClientFactory.CreateClient();
            HttpHelper httpHelper = new HttpHelper();
            try
            {
                httpClient.BaseAddress = new Uri(NavigationManager.BaseUri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await httpClient.GetAsync($"api/Identity/ForgetTwoFactorClient");

                if (!response.IsSuccessStatusCode)
                    throw new Exception(response.ReasonPhrase);

                ToastService.ShowToast("The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
            finally
            {
                httpClient.Dispose();
            }
        }
    }
}