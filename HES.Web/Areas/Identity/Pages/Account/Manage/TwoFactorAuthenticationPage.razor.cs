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
    public partial class TwoFactorAuthenticationPage : ComponentBase
    {
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<TwoFactorAuthenticationPage> Logger { get; set; }

        public TwoFactorInfo TwoFactorInfo { get; set; }
        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await BreadcrumbsService.SetTwoFactorAuthentication();

                var response = await HttpClient.GetAsync("api/Identity/GetTwoFactorInfo");

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                TwoFactorInfo = JsonConvert.DeserializeObject<TwoFactorInfo>(await response.Content.ReadAsStringAsync());

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
                LoadFailed = true;
            }
        }

        private async Task ForgetBrowserAsync()
        {
            try
            {
                var response = await HttpClient.PostAsync("api/Identity/ForgetTwoFactorClient", new StringContent(string.Empty));

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                ToastService.ShowToast("The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }
    }
}