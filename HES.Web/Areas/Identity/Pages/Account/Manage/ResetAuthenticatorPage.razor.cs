using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
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
    public partial class ResetAuthenticatorPage : ComponentBase
    {
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IHttpClientFactory HttpClientFactory { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<ResetAuthenticatorPage> Logger { get; set; }

        public ApplicationUser AppUser { get; set; }


        protected override async Task OnInitializedAsync()
        {
            var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            AppUser = await UserManager.GetUserAsync(state.User);
        }

        private async Task ResetAuthenticatorKeyAsync()
        {
            HttpClient httpClient = HttpClientFactory.CreateClient();

            try
            {
                httpClient.BaseAddress = new Uri(NavigationManager.BaseUri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.GetAsync($"api/Identity/ResetAuthenticatorKey?userId={AppUser.Id}");
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
                httpClient.Dispose();
                NavigationManager.NavigateTo("/Identity/Account/Manage/EnableAuthenticator", false);
            }
        }
    }
}