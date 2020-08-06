using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppUsers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class ProfilePage : ComponentBase
    {
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<ProfilePage> Logger { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
        public ProfileInfo ProfileInfo { get; set; }
        public ProfilePassword ProfilePassword { get; set; }
        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await BreadcrumbsService.SetProfile();

                var response = await HttpClient.GetAsync("api/Identity/GetUser");

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                ApplicationUser = JsonConvert.DeserializeObject<ApplicationUser>(await response.Content.ReadAsStringAsync());

                ProfileInfo = new ProfileInfo
                {
                    Email = ApplicationUser.Email,
                    PhoneNumber = ApplicationUser.PhoneNumber
                };

                ProfilePassword = new ProfilePassword();

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
                LoadFailed = true;
            }
        }

        private async Task SaveProfileAsync()
        {
            try
            {
                var response = await HttpClient.PostAsync("api/Identity/UpdateProfileInfo", new StringContent(JsonConvert.SerializeObject(ProfileInfo), Encoding.UTF8, "application/json"));
                
                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());
   
                ToastService.ShowToast("Your profile has been updated.", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }

        private async Task SendVerificationEmailAsync()
        {
            try
            {
                var response = await HttpClient.PostAsync("api/Identity/SendVerificationEmail", new StringContent(string.Empty));

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                ToastService.ShowToast("Verification email sent.", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }

        public async Task ChangePasswordAsync()
        {
            try
            {
                var response = await HttpClient.PostAsync("api/Identity/UpdateProfilePassword", new StringContent(JsonConvert.SerializeObject(ProfilePassword), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                ToastService.ShowToast("Your password has been changed.", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }
    }
}