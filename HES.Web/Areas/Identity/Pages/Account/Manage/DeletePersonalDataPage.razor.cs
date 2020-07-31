using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class DeletePersonalDataPage : ComponentBase, IDisposable
    {
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IHttpClientFactory HttpClientFactory { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public ILogger<DeletePersonalDataPage> Logger { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        public ApplicationUser AppUser { get; set; }

        private HttpClient _httpClient;

        public InputModel Input { get; set; } = new InputModel();
        public bool RequirePassword { get; set; }
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
            RequirePassword = await UserManager.HasPasswordAsync(AppUser);

            InitializeHttpClient();
        }

        private async Task DeletePersonalDataAsync()
        {
            if (RequirePassword)
            {
                if (!await UserManager.CheckPasswordAsync(AppUser, Input.Password))
                {
                    ToastService.ShowToast("Password not correct.", ToastLevel.Error);
                    return;
                }
            }

            try
            {
                var response = await _httpClient.GetAsync($"api/Identity/DeletePersonalData?userId={AppUser.Id}");
                if (!response.IsSuccessStatusCode)
                    throw new Exception(response.ReasonPhrase);

                Logger.LogInformation($"User with ID '{AppUser.Id}' deleted themselves.");
                ToastService.ShowToast($"User with ID '{AppUser.Id}' deleted themselves.", ToastLevel.Success);
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

    public class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
