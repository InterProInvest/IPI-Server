using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppUsers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class GenerateRecoveryCodesPage : ComponentBase
    {
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<GenerateRecoveryCodesPage> Logger { get; set; }

        public TwoFactorInfo TwoFactorInfo { get; set; }
        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var response = await HttpClient.GetAsync("api/Identity/GetTwoFactorInfo");

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                TwoFactorInfo = JsonConvert.DeserializeObject<TwoFactorInfo>(await response.Content.ReadAsStringAsync());

                if (!TwoFactorInfo.Is2faEnabled)
                {
                    ToastService.ShowToast($"Cannot generate recovery codes for user because they do not have 2FA enabled.", ToastLevel.Error);
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

        private async Task GenerateRecoveryCodesAsync()
        {
            try
            {
                var response = await HttpClient.PostAsync("api/Identity/GenerateNewTwoFactorRecoveryCodes", new StringContent(string.Empty));

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                var recoveryCodes  = JsonConvert.DeserializeObject<List<string>>(await response.Content.ReadAsStringAsync());            

                ToastService.ShowToast("You have generated new recovery codes.", ToastLevel.Success);

                NavigationManager.NavigateTo($"/Identity/Account/Manage/ShowRecoveryCodes/{string.Join("&", recoveryCodes)}", false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }
    }
}