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
    public partial class TwoFactorAuthenticationPage : ComponentBase, IDisposable
    {
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IHttpClientFactory HttpClientFactory { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public ILogger<TwoFactorAuthenticationPage> Logger { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        public TwoFactInformation TwoFactInfo { get; set; }
        public ApplicationUser AppUser { get; set; }

        private HttpClient _httpClient;
        private HttpHelper _httpHelper;

        private void InitializeHttpClient()
        {
            _httpHelper = new HttpHelper();

            _httpClient = HttpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(NavigationManager.BaseUri);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        

        protected override async Task OnInitializedAsync()
        {
            var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            AppUser = await UserManager.GetUserAsync(state.User);

            InitializeHttpClient();

            try
            {
                var response = await _httpClient.GetAsync($"api/Identity/GetTwoFactInformation?userId={AppUser.Id}");
                TwoFactInfo = await _httpHelper.GetObjectFromResponseAsync<TwoFactInformation>(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }

        private async Task ForgetBrowserAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Identity/ForgetTwoFactorClient");
                ToastService.ShowToast("The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            UserManager.Dispose();
        }
    }
}
