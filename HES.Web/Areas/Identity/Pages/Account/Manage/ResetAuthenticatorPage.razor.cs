using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class ResetAuthenticatorPage : ComponentBase, IDisposable
    {
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IHttpClientFactory HttpClientFactory { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public ILogger<ResetAuthenticatorPage> Logger { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        public ApplicationUser AppUser { get; set; }

        private HttpClient _httpClient;

        private void InitializeHttpClient()
        {
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
        }

        private async Task ResetAuthenticatorKeyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Identity/ResetAuthenticatorKey?userId={AppUser.Id}");
                if (!response.IsSuccessStatusCode)
                    throw new Exception(response.ReasonPhrase);

                Logger.LogInformation($"User with ID '{AppUser.Id}' has reset their authentication app key.");
                ToastService.ShowToast("Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
            finally
            {
                NavigationManager.NavigateTo("/Identity/Account/Manage/EnableAuthenticator", false);
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            UserManager.Dispose();
        }
    }
}
