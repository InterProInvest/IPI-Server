using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class DeletePersonalDataPage : ComponentBase
    {
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IHttpClientFactory HttpClientFactory { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<DeletePersonalDataPage> Logger { get; set; }

        public ApplicationUser AppUser { get; set; }
        public InputModel Input { get; set; } = new InputModel();
        public bool RequirePassword { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                AppUser = await UserManager.GetUserAsync(state.User);
                RequirePassword = await UserManager.HasPasswordAsync(AppUser);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
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

            HttpClient httpClient = HttpClientFactory.CreateClient();

            try
            {
                httpClient.BaseAddress = new Uri(NavigationManager.BaseUri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.GetAsync($"api/Identity/DeletePersonalData?userId={AppUser.Id}");
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
            finally
            {
                httpClient.Dispose();
            }
        }
    }

    public class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
